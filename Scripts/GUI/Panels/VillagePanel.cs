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
    [Export] NodePath deckPanelPath;
    TabContainer board;
    Label money;
    public override void _Ready () {
        string save = "";
        try {
            save = FileEncoder.Read();
        } catch (Utils.WrongVersionException e) {
            // GetNode<PopupMessage>("PopupMessage").ShowMessage(e.GetMessage(), () => GetTree().ChangeScene("res://Scenes/TitleScreen.tscn"));
            ThoughtPopup.Instance.OpenCustomMessage(e.GetMessage() + "\n\n[url=~title]Back to the title screen[/url]");
            var callback = Callback.Connect(ThoughtPopup.Instance, "popup_hide", () => GetTree().ChangeScene("res://Scenes/TitleScreen.tscn"));
            callback.CallOnce = true;
            return;
        }
        Loader load = new Loader();
        GameData.Instance = (GameData) load.FromData(save);

        var deckPanel = GetNode<DeckPanel>(deckPanelPath);
        GameData.Instance.Connect(nameof(GameData.DeckChanged), deckPanel, nameof(DeckPanel.ShowDeck));
        GameData.Instance.DeckChange();

        GameData.Instance.State = GameData.GameState.Village;
        SFXHandler.Instance.Change(GameData.GameState.Village);
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
