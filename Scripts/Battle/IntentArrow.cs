using System;
using System.Collections.Generic;
using Godot;

public class IntentArrow : Node2D {
    [Export] NodePath areaPath;
    Control Area;
    YokaiAction Intent;
    Sprite Sprite;

    [Export] List<Texture> Arrows;

    Godot.Color yellowishModulate = new Godot.Color(1, 0.76f, 0.24f, 1);
    public override void _Ready () {
        Sprite = GetNode<Sprite>("Sprite");
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
        Modulate = yellowishModulate;
        switch (action.Type) {
            case YokaiActionType.Attack:
                (Sprite.Texture as AtlasTexture).Region = new Rect2(new Vector2(0, 0), new Vector2(120, 120));
                break;
            case YokaiActionType.AttackAndRemove:
                (Sprite.Texture as AtlasTexture).Region = new Rect2(new Vector2(240, 0), new Vector2(120, 120));
                break;
            case YokaiActionType.Remove:
                (Sprite.Texture as AtlasTexture).Region = new Rect2(new Vector2(120, 0), new Vector2(120, 120));
                break;
            case YokaiActionType.ElementalAttack:
                Modulate = Colors.White;
                Sprite.Texture = Arrows[(int) action.Element - 1];
                break;
            default:
                GD.PrintErr($"{action} : This action's display has not been set");
                break;
        }
        if (action.IsBlocked) {
            Modulate = Colors.White;
            ((ShaderMaterial) Sprite.Material).SetShaderParam("blockAction", true);
        } else {
            ((ShaderMaterial) Sprite.Material).SetShaderParam("blockAction", false);
        }
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