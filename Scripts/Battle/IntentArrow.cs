using System;
using System.Collections.Generic;
using Godot;

public class IntentArrow : Node2D {
    public void ShowArrow (DemonAction action) {
        switch (action) {
            case DemonAction.Attack:
            case DemonAction.AttackOrRemove:
            case DemonAction.AttackPierce:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(0, 0), new Vector2(120, 120));
                break;
            case DemonAction.Remove:
                (GetNode<Sprite>("Sprite").Texture as AtlasTexture).Region = new Rect2(new Vector2(120, 0), new Vector2(120, 120));
                break;

        }
    }

}