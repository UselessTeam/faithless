using System;
using Godot;

public class SeedIcon : Control {
    private static readonly Vector2 SIZE = new Vector2(156, 146);
    private static readonly int WIDTH = 2;
    AtlasTexture texture;
    public override void _Ready () {
        texture = (AtlasTexture) GetNode<TextureRect>("Texture").Texture;
        UpdateValue(0);
    }

    public void UpdateValue (int count) {
        count = Math.Min(count, 4);
        if (count == 0)
            texture.Region = new Rect2(0, 0, 0, 0);
        else {
            count -= 1;
            float x = SIZE.x * (count % WIDTH);
            float y = SIZE.y * (count / WIDTH);
            texture.Region = new Rect2(x, y, SIZE.x, SIZE.y);
        }
    }
}
