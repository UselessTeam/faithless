using System;
using System.Collections.Generic;
using Godot;

public class IntentArrow : Node2D {
    [Export] NodePath areaPath;
    public Control Area;
    public YokaiAction Intent;
    public override void _Ready () {
        Area = GetNode<Control>(areaPath);
        Area.Connect("mouse_entered", this, nameof(MouseEntered));
        Area.Connect("mouse_exited", this, nameof(MouseExited));
        Area.Connect("focus_entered", this, nameof(FocusEnter));
        Area.Connect("focus_exited", this, nameof(FocusExit));
    }
    public void ShowArrow (YokaiAction action) {
        Spatial S = new Spatial();
        var a = S.Transform.basis.x;
        Intent = action;
        switch (action) {
            case YokaiAction.Attack:
            case YokaiAction.AttackPierce:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(0, 0), new Vector2(120, 120));
                break;
            case YokaiAction.AttackOrRemove:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(240, 0), new Vector2(120, 120));
                break;
            case YokaiAction.Remove:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(120, 0), new Vector2(120, 120));
                break;

        }
    }

    [Signal] public delegate void FocusEntered (YokaiAction intent);
    [Signal] public delegate void FocusExited (YokaiAction intent);

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
        EmitSignal(nameof(FocusEntered), Intent);
    }
    private void FocusExit () {
        Modulate = keptColor;
        EmitSignal(nameof(FocusExited), Intent);
    }
}