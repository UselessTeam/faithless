using System;
using Godot;

public class HandContainer : Container {
    [Export] int minWidth;
    public override void _Notification (int notification) {
        float horizontal = Math.Min(1f, RectSize.x / minWidth);
        foreach (Node _child in GetChildren()) {
            if (_child is HBoxContainer child) {
                child.RectScale = new Vector2(horizontal, horizontal);
                // FitChildInRect(child, new Rect2(RectPosition + 0.5f * extra, new Vector2(minX, minY)));
            }
        }
    }
}
