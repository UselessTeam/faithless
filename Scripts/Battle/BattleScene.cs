using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BattleScene : Node2D {
    [Export] Demon currentDemon = new Demon();

    public Control MyHand { get { return GetNode<Control>("Hand"); } }
    public SealingCircle MySealCircle { get { return GetNode<SealingCircle>("SealingCircle"); } }
    static BattleScene instance;
    public static BattleScene Instance {
        get {
            if (instance == null) GD.PrintErr("Battle Scene is null");
            return instance;
        }
    }

    public static List<CardId> Deck;
    public static List<CardId> Hand = new List<CardId>();
    public static List<CardId> Discard = new List<CardId>();

    public static List<Element> SealSlots;

    Label ChiValue { get { return GetNode<Label>("StatsDisplay/ChiValue"); } }
    Label HpValue { get { return GetNode<Label>("StatsDisplay/HpValue"); } }

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

    public override void _Ready () {
        instance = this;
        Deck = GameData.Instance.Deck;

        GetNode<Button>("EndTurn").Connect("button_down", this, nameof(EndPlayerTurn));

        MySealCircle.InitializeSlots(currentDemon.SealSlots);
        Health = MAX_HEALTH;
        SealSlots = Enumerable.Repeat(Element.None, currentDemon.SealSlots).ToList(); ;

        StartPlayerTurn();
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
        UpdateHand();
    }

    void ShuffleDeck () {
        Deck = Utils.RNG.RandomOrder(Deck).ToList();
        GD.Print("AFger shuffle : ", Deck.Count);
    }

    void StartPlayerTurn () {
        Chi = MAX_CHI;
        DrawCards(CARDS_PER_TURN);
        currentState = State.PlayerTurn;
    }

    void EndPlayerTurn () {
        for (byte i = 0 ; i < Hand.Count ; i++) {
            Discard.Add(Hand[i]);
        }
        Hand = new List<CardId>();
        UpdateHand();
        currentState = State.EnemyTurn;

        //TODO Enemi shoud do stuff

        StartPlayerTurn();
    }

    //////////////////
    ////////  Display
    ///////////////
    //////////

    void UpdateHand () {
        for (byte i = 0 ; i < MyHand.GetChildCount() ; i++) MyHand.GetChild(i).QueueFree();
        for (byte i = 0 ; i < Hand.Count ; i++) {
            var makeCard = CardVisual.Instance();
            makeCard.Connect(nameof(CardVisual.OnClick), this, nameof(ClickOnCard));
            MyHand.AddChild(makeCard);
            makeCard.ShowCard(Hand[i].Data());
        }
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
        selectedCard = byte.MaxValue;

        //Switch State
        currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public void AddSeal (Element element, byte location) {
        SealSlots[location] = element;
        // TODO, apply effects
        MySealCircle.UpdateSeals();
    }


}