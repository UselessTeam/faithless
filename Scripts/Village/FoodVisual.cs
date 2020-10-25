using System;
using Godot;

public class FoodVisual : TextureRect {
    [Signal] public delegate void OnClick ();
    public void SetFood (Food food) {
        if (food == null) {
            Hide();
            return;
        }
        var texture = Texture as AtlasTexture;
        var region = texture.Region;
        region.Position = new Vector2(food.Index * 80, 0);
        texture.Region = region;
        Show();
    }

    public override void _GuiInput (InputEvent _event) {
        if (InputHelper.IsClick(_event))
            EmitSignal(nameof(OnClick));
    }
}
