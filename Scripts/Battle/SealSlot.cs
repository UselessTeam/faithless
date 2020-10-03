using System;
using System.Collections.Generic;
using Godot;

public class SealSlot : Control {
    [Signal] delegate void OnClick ();
    public int id;

    public override void _Ready () {
        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnSealSlot));
    }

    public void ShowSlot (bool on_off) { //TODO
        if (on_off) Modulate = new Color("red");
        else Modulate = new Color("black");
    }

    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton)
            EmitSignal(nameof(OnClick), id);
    }

}