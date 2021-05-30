using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class VillagePanel : CanvasLayer {
    [Export] NodePath boardPath;
    [Export] NodePath deckButtonPath;
    [Export] NodePath shopButtonPath;
    [Export] NodePath huntButtonPath;
    [Export] NodePath moneyPath;
    TabContainer board;
    Label money;
    public override void _Ready () {
        string save = FileEncoder.Read();
        Loader load = new Loader();
        GameData.Instance = (GameData) load.FromData(save);

        GameData.Instance.State = GameData.GameState.Village;
        board = GetNode<TabContainer>(boardPath);
        money = GetNode<Label>(moneyPath);
        GetNode<Button>(deckButtonPath).Connect("pressed", board, "set_current_tab", 0.InArray());
        GetNode<Button>(shopButtonPath).Connect("pressed", board, "set_current_tab", 1.InArray());
        GetNode<Button>(huntButtonPath).Connect("pressed", board, "set_current_tab", 2.InArray());
        GameData.Instance.Connect(nameof(GameData.MoneyChanged), this, nameof(SetMoney));
        SetMoney(GameData.Instance.Money);
    }
    public void SetMoney (int value) {
        money.Text = value.ToString();
    }
}
