using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BattleScene : Node2D {
    [Export] Demon currentDemon = new Demon();
    [Export] PackedScene cardVisualPacked;

    public Control MyHand { get { return GetNode<Control>("Hand"); } }
    public SealingCircle MySealCircle { get { return GetNode<SealingCircle>("SealingCircle"); } }
    static BattleScene instance;
    public static BattleScene Instance {
        get {
            if (instance == null) GD.PrintErr("Battle Scene is null");
            return instance;
        }
    }

    public static List<CardId> Deck = new List<CardId>() { CardId.BasicEarth, CardId.BasicFire, CardId.BasicMetal, CardId.BasicWater, CardId.BasicWood };
    public static List<CardId> Hand = new List<CardId>();
    public static List<CardId> Discard = new List<CardId>();

    public static List<Element> SealSlots;

    const byte MAX_CHI = 5;
    const byte MAX_HEALTH = 3;
    const byte CARDS_PER_TURN = 4;

    byte Chi;
    byte Health;

    public enum State { PlayerTurn, CardSelected, EnemyTurn }
    State currentState = State.EnemyTurn;
    byte selectedCard;

    public override void _Ready () {
        instance = this;
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
    }

    void StartPlayerTurn () {
        Chi = MAX_CHI;
        DrawCards(CARDS_PER_TURN);
        currentState = State.PlayerTurn;
    }

    void EndPlayerTurn () {
        for (byte i = 0 ; i < Hand.Count ; i++) {
            Discard.Add(Hand[0]);
            Hand.RemoveAt(0);
        }
        UpdateHand();
        currentState = State.EnemyTurn;
    }

    //////////////////
    ////////  Display
    ///////////////
    //////////

    void UpdateHand () {
        for (byte i = 0 ; i < MyHand.GetChildCount() ; i++) MyHand.GetChild(i).QueueFree();
        for (byte i = 0 ; i < Hand.Count ; i++) {
            var makeCard = (CardVisual) cardVisualPacked.Instance();
            makeCard.id = i;
            MyHand.AddChild(makeCard);
            makeCard.ShowCard(Hand[i].Data());
        }
    }

    /////////////////
    ////////  Input Management
    ////////////////
    //////

    public void ClickOnCard (byte id) {
        selectedCard = id;
        currentState = State.CardSelected;
        GD.Print("Card Selected : " + Hand[id].ToString());
        //TODO Show description and highlight seals
    }

    public void ClickOnSealSlot (byte id) {
        Hand[selectedCard].Data().Use(id);
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