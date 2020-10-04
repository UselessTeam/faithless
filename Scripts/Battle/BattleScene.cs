using System;
using System.Collections.Generic;
using System.Linq;
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
    public short Chi { get { return chi; } set { chi = value; ChiValue.Text = value.ToString(); } }
    public short Health { get { return health; } set { health = value; HpValue.Text = value.ToString(); } }

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

    void DrawCards (byte count) {
        for (byte i = 0 ; i < count ; i++) {
            if (Deck.Count == 0) {
                Deck = Discard;
                Discard = new List<CardId>();
                ShuffleDeck();
            }
            if (Deck.Count == 0) GD.PrintErr("No more cards to draw");
            Hand.Add(Deck[0]);
            Deck.RemoveAt(0);
        }
        DisplayHand();
    }

    void ShuffleDeck () {
        Deck = Utils.RNG.RandomOrder(Deck).ToList();
        GD.Print("AFger shuffle : ", Deck.Count);
    }

    public void StartPlayerTurn () {
        Chi = MAX_CHI;
        DrawCards(CARDS_PER_TURN);
        currentState = State.PlayerTurn;
    }

    void EndPlayerTurn () {
        for (byte i = 0 ; i < Hand.Count ; i++) {
            Discard.Add(Hand[i]);
        }
        Hand = new List<CardId>();
        DisplayHand();
        currentState = State.EnemyTurn;

        MySealCircle.PlanNextDemonTurn();
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

    /////////////////
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
        MyHand.GetChild<CanvasItem>(selectedCard).QueueFree();
        Discard.Add(Hand[selectedCard]);
        Hand.RemoveAt(selectedCard);
        DisplayDeckAndDiscard();

        //Switch State
        selectedCard = byte.MaxValue;
        currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public void AddSeal (Element element, byte location) {
        SealSlots[location] = element;
        // TODO, apply effects
        MySealCircle.DisplaySeals();
    }


}