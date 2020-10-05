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

    [Signal] public delegate void ChangeGameState (GameState value);
    private GameState _state = GameState.Narration;
    public GameState State {
        get => _state;
        set {
            if (value != _state) {
                EmitSignal(nameof(ChangeGameState), value);
                _state = value;
            }
        }
    }

    [Signal] public delegate void DeckChanged (int value);
    public List<CardId> Deck = new List<CardId>() {
        CardId.BasicEarth, CardId.BasicEarth,
        CardId.BasicFire, CardId.BasicFire,
        CardId.BasicMetal, CardId.BasicMetal,
        CardId.BasicWater, CardId.BasicWater,
        CardId.BasicWood, CardId.BasicWood,
    };
    public void DeckChange () {
        EmitSignal(nameof(DeckChanged));
    }
    public Demon Oni = HuntPanel.DemonList[0];

    [Signal] public delegate void MoneyChanged (int value);

    public List<Food> LeftInShop = new List<Food> {
        Food.ONIGIRI,
        Food.SUSHI,
        Food.DANGO,
        Food.JAGAIMO
    };
    public short MaxHealth = 3;
    public byte CardsPerTurn = 4;
    public short MaxKi = 5;
    public int MoneyPercentageBonus = 0;
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