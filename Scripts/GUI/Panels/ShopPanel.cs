using System;
using Godot;

public class ShopPanel : Container {
    public override void _Ready () {
    }

    public override void _Notification (int notification) {
        foreach (Node _child in GetChildren()) {
            if (_child is Control child) {
                float minX = Math.Max(1, child.RectMinSize.x);
                float minY = Math.Max(1, child.RectMinSize.y);
                float scale_x = RectSize.x / minX;
                float scale_y = RectSize.y / minY;
                float scale = Math.Min(scale_x, scale_y);
                Vector2 size = new Vector2(minX * scale, minY * scale);
                Vector2 extra = RectSize - size;
                FitChildInRect(child, new Rect2(RectPosition + 0.5f * extra, new Vector2(minX, minY)));
                child.RectScale = new Vector2(scale, scale);
            }
        }
    }
}
