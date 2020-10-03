using System;
using Godot;
using Utils;

public class ThoughtPopup : Popup {
    public static ThoughtPopup Instance { get; private set; }
    SmartText text;

    public override void _Ready () {
        Instance = this;
        text = GetNode<SmartText>("ThoughtText/Margin/Text");
    }

    public void Open (string info) {
        PopupCentered();
        text.BbcodeText = info;
    }

    public override void _GuiInput (InputEvent @event) {
        if (@event is InputEventMouseButton eventMouse && eventMouse.ButtonIndex == (int) ButtonList.Left && eventMouse.Pressed) {
            Hide();
        }
    }
}
