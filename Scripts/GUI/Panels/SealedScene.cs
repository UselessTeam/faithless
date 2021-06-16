using System;
using Godot;
using Utils;

public class SealedScene : ColorRect {

    [Export] NodePath moneyPath;
    [Export] NodePath cardPath;
    [Export] NodePath descriptionPath;
    [Export] NodePath addCardbuttonPath;
    [Export] NodePath continuePath;
    public override void _Ready () {
        GameData.Instance.State = GameData.GameState.Narration;
        if (GameData.Instance.yokai == YokaiId.None) {
            Continue();
            return;
        } // Sanity check to avoid the infamous win-crash
        int money = (100 + GameData.Instance.MoneyPercentageBonus) * GameData.Instance.yokai.Data().Reward / 100;
        GameData.Instance.yokai = YokaiId.None;
        GetNode<Label>(moneyPath).Text = $"+{money}";
        GameData.Instance.Money += money;

        Global.SaveGame();

        CardId card = CardData.AllSpecial.Random();
        GetNode<CardVisual>(cardPath).ShowCard(card.Data());
        GetNode<CardVisual>(cardPath).IsDisabled = true;
        GetNode<RichTextLabel>(descriptionPath).BbcodeText = BB.Format(card.Data().Description);
        GetNode<Button>(addCardbuttonPath).Connect("pressed", this, nameof(AddToDeck), new Godot.Collections.Array() { card });
        GetNode<Button>(continuePath).Connect("pressed", this, nameof(Continue));
    }

    public void Continue () {
        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");

        this.QueueFree();
    }

    public void AddToDeck (CardId card) {
        GetNode<Button>(addCardbuttonPath).Disabled = true;
        GameData.Instance.Deck.Add(card);
        Global.SaveGame();
    }
}
