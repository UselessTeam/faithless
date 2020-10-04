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
    [Export] NodePath buttonPath;
    public override void _Ready () {
        Demon oni = GameData.Instance.Oni;
        GameData.Instance.Oni = null;
        GetNode<Label>(moneyPath).Text = $"+{oni.Reward}";
        GameData.Instance.Money += oni.Reward;
        CardId card = Card.AllSpecial.Random();
        GetNode<CardVisual>(cardPath).ShowCard(card.Data());
        GetNode<RichTextLabel>(descriptionPath).BbcodeText = card.Data().Description;
        GameData.Instance.Deck.Add(card);
        GetNode<Button>(buttonPath).Connect("pressed", this, nameof(Continue));
    }

    public void Continue () {
        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");
    }
}
