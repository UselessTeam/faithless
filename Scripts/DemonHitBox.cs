using System;
using Godot;

public class DemonHitBox : Control {
    [Signal] public delegate void OnClick (int id);

    public override void _Ready () {
        // Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnTarget));
        BattleScene.demonHitBoxPath = GetPath();
        // GD.Print(this.GetPath());
    }

    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton)
            EmitSignal(nameof(OnClick), -1);
    }
}
