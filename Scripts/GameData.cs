using System;
using System.Collections.Generic;
using Godot;

public class GameData : Node2D {
    public static GameData Instance;

    public List<CardId> Deck = new List<CardId>() { CardId.BasicEarth, CardId.BasicFire, CardId.BasicMetal, CardId.BasicWater, CardId.BasicWood };

    public override void _Ready () {
        Instance = this;
    }
}