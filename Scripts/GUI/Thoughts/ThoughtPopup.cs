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

    public void Open (string key) {
        PopupCentered();
        text.BbcodeText = $"[center]{BB.Format(Help.Explanations[key])}[/center]";
    }

    public override void _GuiInput (InputEvent @event) {
        if (@event is InputEventMouseButton eventMouse && eventMouse.ButtonIndex == (int) ButtonList.Left && eventMouse.Pressed) {
            Hide();
        }
    }

    public override void _UnhandledInput (InputEvent @event) {
        if (@event.IsActionPressed("ui_cancel")) {
            switch (GameData.Instance.State) {
                case GameData.GameState.Battle:
                    Open("help-battle");
                    break;
                case GameData.GameState.Village:
                    Open("help-village");
                    break;
            }
        }
    }
}
