using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class BattleScene : MarginContainer {

    static BattleScene instance;
    public static BattleScene Instance {
        get {
            if (instance == null) GD.PrintErr("Battle Scene is null");
            return instance;
        }
    }

    /*** Nodes ***/
    [Export] NodePath endTurnPath;
    [Export] NodePath chiPath;
    [Export] NodePath hpPath;
    [Export] NodePath deckPath;
    [Export] NodePath discardPath;
    [Export] NodePath handPath;
    [Export] NodePath deleteHandPath;
    [Export] NodePath sealingCirclePath;

    Label chiField;
    Label hpField;
    Label deckField;
    Label discardField;
    public Control HandField;
    public Control DeleteHandField;
    public SealingCircle SealCircleField;


    /*** Others ***/
    [Export] Demon currentDemon = new Demon() { SealSlots = 6 };

    public static List<CardId> Deck;

    public static List<CardId> Hand = new List<CardId>();
    public static List<CardId> Discard = new List<CardId>();

    public static List<Element> SealSlots;

    const byte MAX_CHI = 5;
    const byte MAX_HEALTH = 3;
    const byte CARDS_PER_TURN = 4;

    short chi;
    short health;
    public static short Chi { get { return Instance.chi; } set { Instance.chi = value; Instance.chiField.Text = value.ToString(); } }
    public static short Health {
        get { return Instance.health; }
        set {
            Instance.health = value; Instance.hpField.Text = value.ToString();
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

        chiField = GetNode<Label>(chiPath);
        hpField = GetNode<Label>(hpPath);
        deckField = GetNode<Label>(deckPath);
        discardField = GetNode<Label>(discardPath);

        HandField = GetNode<Control>(handPath);
        DeleteHandField = GetNode<Control>(deleteHandPath);
        SealCircleField = GetNode<SealingCircle>(sealingCirclePath);

        GetNode<Button>(endTurnPath).Connect("button_down", this, nameof(EndPlayerTurn));

        SealCircleField.InitializeSlots(currentDemon.SealSlots);
        Health = MAX_HEALTH;
        SealSlots = Enumerable.Repeat(Element.None, currentDemon.SealSlots).ToList(); ;

        SealCircleField.PlanNextDemonTurn(); // This function will start the player's turn once it's done
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
            Instance.HandField.AddChild(makeCard);
            makeCard.Modulate = new Color(1, 1, 1, 0);
            makeCard.ShowCard(addCard.Data());
            makeCard.MoveFrom(new Vector2(1000, 0));
            await Instance.ToSignal(Instance.HandField.GetChild<CardVisual>(Instance.HandField.GetChildCount() - 1).MyTween, "tween_completed");
        }
        Instance.HandField.Hide();
        Instance.HandField.Show();
    }

    public static async void DiscardCard (byte index) {
        var toDiscard = Instance.HandField.GetChild<CardVisual>(index);
        toDiscard.IsDisabled = true;
        Discard.Add(Hand[index]);
        Hand.RemoveAt(index);
        Instance.HandField.RemoveChild(toDiscard);
        Instance.DeleteHandField.AddChild(toDiscard);

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
        await SealCircleField.PlayDemonTurn();
    }

    //////////////////
    ////////  Display
    ///////////////
    //////////

    void DisplayHand () {
        HandField.QueueFreeChildren();
        for (byte i = 0 ; i < Hand.Count ; i++) {
            var makeCard = CardVisual.Instance();
            makeCard.Connect(nameof(CardVisual.OnClick), this, nameof(ClickOnCard));
            HandField.AddChild(makeCard);
            makeCard.ShowCard(Hand[i].Data());
        }
        DisplayDeckAndDiscard();
    }

    void DisplayDeckAndDiscard () {
        deckField.Text = Deck.Count.ToString();
        discardField.Text = Discard.Count.ToString();
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    public void ClickOnCard (byte id) {
        // Unselect previous card
        if (selectedCard < byte.MaxValue)
            HandField.GetChild<CanvasItem>(selectedCard).Modulate = new Color(1, 1, 1, 1);

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
        await SealCircleField.DisappearSeal(location);
    }

    public async Task SwitchSeal (Element element, byte location) {
        bool isEmpty = SealSlots[location] == Element.None;
        SealSlots[location] = element;
        if (isEmpty)
            await SealCircleField.AppearSeal(location);
        else
            await SealCircleField.ReplaceSeal(location);
    }

    public async Task AddSeal (Element element, byte location) {
        var OldElement = SealSlots[location];
        SealSlots[location] = element;
        Task task;
        if (OldElement == Element.None)
            task = SealCircleField.AppearSeal(location);
        else task = SealCircleField.ReplaceSeal(location);


        if (element == Element.Earth && SealSlots.Contains(Element.None)) {
            int moveLocation = location;
            var moveElement = OldElement;
            if (moveElement == Element.None) {
                OldElement = Element.Earth;
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


        if (!SealSlots.Contains(Element.None) && currentDemon.CheckWinCondition())
            GD.Print("TODO: Win!");

        await task;

        if (element == Element.Water && OldElement == Element.Fire)
            Health += 1;
        if (element == Element.Fire && OldElement == Element.Wood)
            Chi += 1;
        if (element == Element.Wood && OldElement == Element.Water)
            await DrawCards(1);

        SealCircleField.DisplaySeals(); // Sanity check, just in case
    }

    async public Task StartTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Fire
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Fire) {// If there is a fire after or before
                    Chi += 1;

                    await SwitchSeal(Element.Fire, (byte) i);
                    SealCircleField.DisplaySeals(); //Sanity check
                }
            }
        }
    }
    async public Task EndTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Water
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Water) {// If there is a water after or before
                    // TODO: show a cute water effect on the wood
                    await DrawCards(1);
                }
            }
        }
    }



}