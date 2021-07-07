using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Utils;

public class GameData : Node2D, ISaveable {
    public static GameData Instance;
    [Signal] public delegate void InstanceLoaded (int value);

    public enum GameState {
        None,
        TitleScreen,
        Narration,
        Village,
        Battle
    }

    [Signal] public delegate void ChangeGameState (GameState value);
    private GameState _state = GameState.TitleScreen;
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
    public void DeckChange () {
        EmitSignal(nameof(DeckChanged));
    }
    [Save]
    public List<CardId> Deck = CardData.DefaultDeck();
    public YokaiId yokai = YokaiId.Hitotsumekozo;

    [Signal] public delegate void MoneyChanged (int value);

    [Save]
    public List<FoodId> LeftInShop = new List<FoodId> {
        FoodId.Onigiri,
        FoodId.Sushi,
        FoodId.Dango,
        FoodId.Jagaimo
    };
    [Save] public short MaxHealth = 3;
    [Save] public int CardsPerTurn = 4;
    [Save] public short MaxKi = 5;
    [Save] public int MoneyPercentageBonus = 0;
    public int Money {
        get => _money;
        set {
            EmitSignal(nameof(MoneyChanged), value);
            _money = value;
        }
    }
    [Save] private int _money = 300;

    public override void _Ready () {
        Instance = this;
    }
}