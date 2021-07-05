using System;
using Godot;

public class LostScene : ColorRect {
    [Export] NodePath buttonPath;
    public override void _Ready () {
        GameData.Instance.State = GameData.GameState.Narration;
        GetNode<Button>(buttonPath).Connect("pressed", this, nameof(Continue));
    }
    public void Continue () {
        Utils.RNG.StartCycle();
        GetTree().ChangeScene("res://Scenes/VillageScene.tscn");
    }
}
