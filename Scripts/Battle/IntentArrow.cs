using System;
using System.Collections.Generic;
using Godot;

public class IntentArrow : Node2D {
    [Export] NodePath areaPath;
    public Control Area;
    public YokaiAction Intent;

    Godot.Color originalModulate;
    Godot.Color blockedModulate = new Godot.Color(0.9f, 0.9f, 0.9f);
    public override void _Ready () {
        Area = GetNode<Control>(areaPath);
        Area.Connect("mouse_entered", this, nameof(MouseEntered));
        Area.Connect("mouse_exited", this, nameof(MouseExited));
        Area.Connect("focus_entered", this, nameof(FocusEnter));
        Area.Connect("focus_exited", this, nameof(FocusExit));
        originalModulate = Modulate;
    }
    public void ShowArrow (YokaiAction action) {
        Spatial S = new Spatial();
        var a = S.Transform.basis.x;
        Intent = action;
        switch (action.Type) {
            case YokaiActionType.Attack:
            case YokaiActionType.AttackPierce:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(0, 0), new Vector2(120, 120));
                break;
            case YokaiActionType.AttackOrRemove:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(240, 0), new Vector2(120, 120));
                break;
            case YokaiActionType.Remove:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(120, 0), new Vector2(120, 120));
                break;
            default:
                GD.PrintErr($"{action} : This action's display has not been set");
                break;
        }
        if (action.IsBlocked)
            Modulate = blockedModulate;
        else
            Modulate = originalModulate;
    }

    [Signal] public delegate void FocusEntered (string actionDescription);
    [Signal] public delegate void FocusExited (string actionDescription);

    private void MouseEntered () {
        Area.GrabFocus();
    }
    private void MouseExited () {
        Area.ReleaseFocus();
    }

    Color keptColor;
    private void FocusEnter () {
        keptColor = Modulate;
        Color m = keptColor;
        m.r *= 1.2f;
        m.g *= 1.2f;
        m.b *= 1.2f;
        Modulate = m;
        EmitSignal(nameof(FocusEntered), Intent.Description());
    }
    private void FocusExit () {
        Modulate = keptColor;
        EmitSignal(nameof(FocusExited), Intent.Description());
    }
}