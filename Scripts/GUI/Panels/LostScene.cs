using System;
using Godot;

public class LostScene : ColorRect {
    public static void Loose (SceneTree tree) {
        LostScene instance = (LostScene) ResourceLoader.Load<PackedScene>("res://Scenes/LostScene.tscn").Instance();
        tree.Root.AddChild(instance);
    }
    [Export] NodePath buttonPath;
    public override void _Ready () {
        GetNode<Button>(buttonPath).Connect("pressed", this, nameof(Continue));
    }
    public void Continue () {
        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");
    }
}
