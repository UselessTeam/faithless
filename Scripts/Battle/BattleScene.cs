using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BattleScene : Node2D {
    [Export] Demon currentDemon = new Demon();
    [Export] PackedScene cardVisualPacked;

    public Control MyHand { get { return GetNode<Control>("Hand"); } }

    List<CardId> Deck = new List<CardId>() { CardId.BasicEarth, CardId.BasicFire, CardId.BasicMetal, CardId.BasicWater, CardId.BasicWood };
    List<CardId> Hand = new List<CardId>();
    List<CardId> Discard = new List<CardId>();

    const byte MAX_CHI = 5;
    const byte MAX_HEALTH = 3;
    const byte CARDS_PER_TURN = 4;

    byte Chi;
    byte Health;

    public override void _Ready () {
        GetNode<SealingCircle>("SealingCircle").ShowSlots(currentDemon.SealSlots);
        Health = MAX_HEALTH;
        StartPlayerTurn();
    }

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
    }

    void EndPlayerTurn () {
        for (byte i = 0 ; i < Hand.Count ; i++) {
            Discard.Add(Hand[0]);
            Hand.RemoveAt(0);
        }
        UpdateHand();
    }

    void UpdateHand () {
        for (byte i = 0 ; i < MyHand.GetChildCount() ; i++) MyHand.GetChild(i).QueueFree();
        for (byte i = 0 ; i < Hand.Count ; i++) {
            var makeCard = (CardVisual) cardVisualPacked.Instance();
            makeCard.ShowCard(Hand[i]);
            MyHand.AddChild(makeCard);
        }
    }


    /////////////////
    ////////  Input Management
    ////////////////
    //////

    public void ClickOnCard (int id) { }
    public void ClickOnSealSlot (int id) { }

}