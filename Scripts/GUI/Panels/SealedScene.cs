using System;
using Godot;
using Utils;

public class SealedScene : ColorRect {

    public static void Win (SceneTree tree) {
        SealedScene instance = (SealedScene) ResourceLoader.Load<PackedScene>("res://Scenes/SealedScene.tscn").Instance();
        tree.Root.AddChild(instance);
    }
    [Export] NodePath moneyPath;
    [Export] NodePath cardPath;
    [Export] NodePath descriptionPath;
    [Export] NodePath addCardbuttonPath;
    [Export] NodePath continuePath;
    public override void _Ready () {
        GameData.Instance.State = GameData.GameState.Narration;
        Demon oni = GameData.Instance.Oni;
        GameData.Instance.Oni = null;
        int money = (100 + GameData.Instance.MoneyPercentageBonus) * oni.Reward / 100;
        GetNode<Label>(moneyPath).Text = $"+{money}";
        GameData.Instance.Money += money;
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
        // GetTree().Paused = false;
    }

    public void AddToDeck (CardId card) {
        GetNode<Button>(addCardbuttonPath).Disabled = true;
        GameData.Instance.Deck.Add(card);
    }
}
