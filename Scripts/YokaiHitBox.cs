using System;
using Godot;

public class YokaiHitBox : Control {
    [Signal] public delegate void OnClick (int id);
    private TextureRect glow;

    public override void _Ready () {
        glow = GetNode<TextureRect>("Glow");
        BattleScene.yokaiHitBoxPath = GetPath();
    }

    public void StartGlow () {
        MouseDefaultCursorShape = CursorShape.PointingHand;
        glow.Show();
    }
    public void StopGlow () {
        MouseDefaultCursorShape = CursorShape.Arrow;
        glow.Hide();
    }

    public override void _GuiInput (InputEvent _event) {
        if (InputHelper.IsClick(_event))
            EmitSignal(nameof(OnClick), -1);
    }
}
