using System;
using System.Collections.Generic;
using Godot;

public class GameData : Node2D {
    public static GameData Instance;

    public enum GameState {
        Narration,
        Village,
        Battle
    }

    public GameState State = GameState.Narration;

    [Signal] public delegate void DeckChanged (int value);
    public List<CardId> Deck = new List<CardId>() { CardId.BasicEarth, CardId.BasicEarth,
                                                    CardId.BasicFire, CardId.BasicFire,
                                                    CardId.BasicMetal, CardId.BasicMetal,
                                                    CardId.BasicWater, CardId.BasicWater,
                                                    CardId.BasicWood, CardId.BasicWood,
                                                   };
    public void DeckChange () {
        EmitSignal(nameof(DeckChanged));
    }
    public Demon Oni = new Demon {
        Name = "Default Oni",
        SealSlots = 6,
    };

    [Signal] public delegate void MoneyChanged (int value);
    public int Money {
        get => _money;
        set {
            EmitSignal(nameof(MoneyChanged), value);
            _money = value;
        }
    }
    private int _money = 300;

    public override void _Ready () {
        Instance = this;
    }
}