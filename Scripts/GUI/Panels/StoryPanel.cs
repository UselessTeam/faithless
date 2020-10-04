using System;
using Godot;
using Utils;

public class StoryPanel : ColorRect {
    [Export] NodePath textPath;
    [Export] float time;
    [Export(PropertyHint.File)] string nextScene;
    SmartText text;
    public override void _Ready () {
        text = GetNode<SmartText>(textPath);
    }

    float progress;
    public override void _Process (float delta) {
        progress += delta / time;
        progress = Math.Min(1f, progress);
        text.PercentVisible = progress;
    }

    public override void _Input (InputEvent @event) {
        if (progress >= 1f && @event is InputEventMouseButton eventMouse && eventMouse.ButtonIndex == (int) ButtonList.Left && eventMouse.Pressed) {
            GetTree().ChangeScene(nextScene);
        }
    }
}
