using System;
using Godot;

public class LostScene : ColorRect {
    public static void Lose (SceneTree tree) {
        LostScene instance = (LostScene) ResourceLoader.Load<PackedScene>("res://Scenes/LostScene.tscn").Instance();
        tree.Root.AddChild(instance);
    }
    [Export] NodePath buttonPath;
    public override void _Ready () {
        GameData.Instance.State = GameData.GameState.Narration;
        GetNode<Button>(buttonPath).Connect("pressed", this, nameof(Continue));
    }
    public void Continue () {
        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");
    }
}
