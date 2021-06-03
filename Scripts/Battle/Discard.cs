using System;
using System.Collections.Generic;
using Godot;

public class Discard : Control {
    CardVisual[] displayCards = new CardVisual[3];

    public override void _Ready () {
        base._Ready();
        for (int i = 0 ; i < 3 ; i++)
            displayCards[i] = GetNode<CardVisual>("Card" + i);
    }

    public void UpdateDiscard () {
        var discard = BattleScene.Cards.Discard;
        for (int i = 0 ; i < 3 ; i++) {
            if (discard.Count <= i)
                displayCards[i].Hide();
            else
                displayCards[i].ShowCard(discard[discard.Count - 1 - i].Data());
        }
    }

}
