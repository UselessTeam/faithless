using System;
using Godot;
using Utils;

public class StoryPanel : ColorRect {
    [Export] NodePath textPath;
    [Export] float time;
    [Export] float speedUp = 10f;
    [Export(PropertyHint.File)] string nextScene;
    SmartText text;
    public override void _Ready () {
        text = GetNode<SmartText>(textPath);
    }

    float progress;
    public override void _Process (float delta) {
        float factor = Input.IsActionPressed("ui_accept") || Input.IsActionPressed("ui_select") || Input.IsMouseButtonPressed(0) || Input.IsMouseButtonPressed(1) ? speedUp : 1f;
        progress += factor * delta / time;
        progress = Math.Min(1f, progress);
        text.PercentVisible = progress;
    }

    public override void _Input (InputEvent _event) {
        if (progress >= 1f && InputHelper.IsClick(_event)) {
            GetTree().ChangeScene(nextScene);
        }
    }
}
