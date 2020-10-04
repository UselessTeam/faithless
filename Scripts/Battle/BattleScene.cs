using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class BattleScene : Node2D {

    static BattleScene instance;
    public static BattleScene Instance {
        get {
            if (instance == null) GD.PrintErr("Battle Scene is null");
            return instance;
        }
    }

    [Export] Demon currentDemon = new Demon() { SealSlots = 6 };
    [Export] PackedScene cardVisualPacked;

    public Control MyHand { get { return GetNode<Control>("Hand"); } }
    public SealingCircle MySealCircle { get { return GetNode<SealingCircle>("SealingCircle"); } }

    public static List<CardId> Deck;

    public static List<CardId> Hand = new List<CardId>();
    public static List<CardId> Discard = new List<CardId>();

    public static List<Element> SealSlots;

    Label ChiValue { get { return GetNode<Label>("StatsDisplay/ChiValue"); } }
    Label HpValue { get { return GetNode<Label>("StatsDisplay/HpValue"); } }
    Label DeckValue { get { return GetNode<Label>("StatsDisplay/DeckValue"); } }
    Label DiscardValue { get { return GetNode<Label>("StatsDisplay/DiscardValue"); } }

    const byte MAX_CHI = 5;
    const byte MAX_HEALTH = 3;
    const byte CARDS_PER_TURN = 4;

    short chi;
    short health;
    public static short Chi { get { return Instance.chi; } set { Instance.chi = value; Instance.ChiValue.Text = value.ToString(); } }
    public static short Health {
        get { return Instance.health; }
        set {
            Instance.health = value; Instance.HpValue.Text = value.ToString();
            if (Health <= 0) GD.Print("TODO: Die");
        }
    }

    public enum State { PlayerTurn, CardSelected, EnemyTurn }
    State currentState = State.EnemyTurn;
    byte selectedCard = byte.MaxValue;

    /////Initialization
    ///

    public override void _Ready () {
        instance = this;
        Deck = GameData.Instance.Deck;

        GetNode<Button>("EndTurn").Connect("button_down", this, nameof(EndPlayerTurn));

        MySealCircle.InitializeSlots(currentDemon.SealSlots);
        Health = MAX_HEALTH;
        SealSlots = Enumerable.Repeat(Element.None, currentDemon.SealSlots).ToList(); ;

        MySealCircle.PlanNextDemonTurn(); // This function will start the player's turn once it's done
    }

    ///////////////////
    //////  Cards Management
    ////////////////
    ///////

    public static async Task DrawCards (byte count) {
        for (byte i = 0 ; i < count ; i++) {
            if (Deck.Count == 0) {
                Deck = Discard;
                Discard = new List<CardId>();
                Instance.ShuffleDeck();
            }
            if (Deck.Count == 0) GD.PrintErr("No more cards to draw");
            var addCard = Deck[0];
            Hand.Add(addCard);
            Deck.RemoveAt(0);

            var makeCard = CardVisual.Instance();
            makeCard.Connect(nameof(CardVisual.OnClick), Instance, nameof(ClickOnCard));
            Instance.MyHand.AddChild(makeCard);
            makeCard.Modulate = new Color(1, 1, 1, 0);
            makeCard.ShowCard(addCard.Data());
            makeCard.MoveFrom(new Vector2(1000, 0));
            await Instance.ToSignal(Instance.MyHand.GetChild<CardVisual>(Instance.MyHand.GetChildCount() - 1).MyTween, "tween_completed");
        }
        Instance.MyHand.Hide();
        Instance.MyHand.Show();
    }

    public static async void DiscardCard (byte index) {
        var toDiscard = Instance.MyHand.GetChild<CardVisual>(index);
        toDiscard.IsDisabled = true;
        Discard.Add(Hand[index]);
        Hand.RemoveAt(index);
        Instance.MyHand.RemoveChild(toDiscard);
        Instance.GetNode("ToDeleteHand").AddChild(toDiscard);

        toDiscard.Disappear();
        await Instance.ToSignal(toDiscard.MyTween, "tween_completed");
        toDiscard.QueueFree();
        Instance.DisplayDeckAndDiscard();
    }

    void ShuffleDeck () {
        Deck = Utils.RNG.RandomOrder(Deck).ToList();
    }

    async public void StartPlayerTurn () {
        Chi = MAX_CHI;
        await DrawCards(CARDS_PER_TURN);
        await StartTurnEffects();
        currentState = State.PlayerTurn;
    }

    async void EndPlayerTurn () {
        if (currentState == State.EnemyTurn) return;
        while (Hand.Count > 0) {
            DiscardCard(0);
        }
        currentState = State.EnemyTurn;
        await EndTurnEffects();
        MySealCircle.PlayDemonTurn();
    }

    //////////////////
    ////////  Display
    ///////////////
    //////////

    void DisplayHand () {
        MyHand.QueueFreeChildren();
        for (byte i = 0 ; i < Hand.Count ; i++) {
            var makeCard = CardVisual.Instance();
            makeCard.Connect(nameof(CardVisual.OnClick), this, nameof(ClickOnCard));
            MyHand.AddChild(makeCard);
            makeCard.ShowCard(Hand[i].Data());
        }
        DisplayDeckAndDiscard();
    }

    void DisplayDeckAndDiscard () {
        DeckValue.Text = Deck.Count.ToString();
        DiscardValue.Text = Discard.Count.ToString();
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    public void ClickOnCard (byte id) {
        // Unselect previous card
        if (selectedCard < byte.MaxValue)
            MyHand.GetChild<CanvasItem>(selectedCard).Modulate = new Color(1, 1, 1, 1);

        if (Hand[id].Data().Cost > Chi) {
            GD.Print("Not enough Chi");
            // TODO : Not enough chi
            return;
        }
        // Logic selection
        selectedCard = id;
        currentState = State.CardSelected;
        GD.Print("Card Selected : " + Hand[id].ToString());

        // Display selection
        // MyHand.GetChild<CanvasItem>(selectedCard).Modulate = new Color(1, 1, 0.6f, 0.9f);
        //TODO Show description and highlight seals

    }

    public void ClickOnSealSlot (byte id) {
        if (currentState != State.CardSelected) return;

        //Use the card
        Chi -= Hand[selectedCard].Data().Cost;
        if (Chi < 0) GD.PrintErr("NegativeChi");
        Hand[selectedCard].Data().Use(id);

        // Discard the Card
        DiscardCard(selectedCard);

        //Switch State
        selectedCard = byte.MaxValue;
        currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public async Task RemoveSeal (byte location) {
        SealSlots[location] = Element.None;
    }

    public async Task SwitchSeal (Element element, byte location) {
        SealSlots[location] = element;
    }

    public async Task AddSeal (Element element, byte location) {
        if (element == Element.Water && SealSlots[location] == Element.Fire)
            Health += 1;

        if (element == Element.Fire && SealSlots[location] == Element.Wood)
            Chi += 1;

        if (element == Element.Wood && SealSlots[location] == Element.Water)
            DrawCards(1);

        if (element == Element.Earth && SealSlots.Contains(Element.None)) {
            int moveLocation = location;
            var moveElement = SealSlots[location];
            if (moveElement == Element.None) {
                SealSlots[location] = Element.Earth;
                moveLocation = (moveLocation + 1) % SealSlots.Count;
                moveElement = SealSlots[moveLocation];
                if (!SealSlots.Contains(Element.None))
                    moveElement = Element.None;
                else
                    SealSlots[moveLocation] = Element.None;
            }
            while (moveElement != Element.None) {
                moveLocation = (moveLocation + 1) % SealSlots.Count;
                var tempSwap = SealSlots[moveLocation];
                SealSlots[moveLocation] = moveElement;
                moveElement = tempSwap;
            }
        }


        SealSlots[location] = element;

        if (!SealSlots.Contains(Element.None) && currentDemon.CheckWinCondition())
            GD.Print("TODO: Win!");

        MySealCircle.DisplaySeals();
    }

    async public Task StartTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Fire
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Fire) {// If there is a fire after or before
                    Chi += 1;
                    SealSlots[i] = Element.Fire;
                    MySealCircle.DisplaySeals();
                }
            }
        }
    }
    async public Task EndTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Water
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Water) {// If there is a water after or before
                    DrawCards(1);
                }
            }
        }
    }



}