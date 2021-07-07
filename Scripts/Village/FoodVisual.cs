using System;
using Godot;
using Utils;

public class FoodVisual : TextureRect {
    [Signal] public delegate void OnClick ();
    public void SetFood (FoodData food) {
        if (food == null) {
            Hide();
            return;
        }
        var texture = Texture as AtlasTexture;
        var region = texture.Region;
        region.Position = new Vector2(((int) food.Id - 1) * 80, 0);
        texture.Region = region;
        GetNode<SmartText>("Text").BbcodeText = $"[center]{food.Price} [img]res://Assets/Sprites/GUI/mon_icon.png[/img][/center]";

        Show();
    }

    public override void _GuiInput (InputEvent _event) {
        if (InputHelper.IsClick(_event))
            EmitSignal(nameof(OnClick));
    }
}
