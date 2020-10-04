using System;
using Godot;

public class HandContainer : Container {
    [Export] int minWidth;
    int cards = 4;
    const int CARDS_WIDTH = 180;
    public void SetCards (int total) {
        cards = total;
        Resize();
    }
    public override void _Notification (int notification) {
        Resize();
    }

    public void Resize () {
        float horizontal = Math.Min(1f, RectSize.x / minWidth);
        float width = Math.Max(RectSize.x, minWidth);
        float split = (width - cards * CARDS_WIDTH);
        if (split < 0) {
            split /= cards - 1;
        } else {
            split = 0;
        }
        foreach (Control holder in GetChildren()) {
            holder.RectScale = new Vector2(horizontal, horizontal);
            foreach (HBoxContainer _child in holder.GetChildren()) {
                if (_child is HBoxContainer child) {
                    child.Set("custom_constants/separation", split);
                }
            }
        }
    }
}
