using System;
using System.Collections.Generic;
using Godot;

public class GameData : Node2D {
    public static GameData Instance;

    public List<CardId> Deck = new List<CardId>() { CardId.BasicEarth, CardId.BasicFire, CardId.BasicMetal, CardId.BasicWater, CardId.BasicWood };
    public Demon Oni = new Demon {
        Name = "Default Oni",
        SealSlots = 6,
    };

    public override void _Ready () {
        Instance = this;
    }
}