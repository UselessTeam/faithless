using System;
using Godot;

public class SeedIcon : Control {
    private static readonly float SIZE = 128f;
    private static readonly int WIDTH = 2;
    AtlasTexture texture;
    public override void _Ready () {
        texture = (AtlasTexture) GetNode<TextureRect>("Texture").Texture;
    }

    public void UpdateValue (int count) {
        count = Math.Min(count, 4);
        float x = SIZE * (count % WIDTH);
        float y = SIZE * (count / WIDTH);
        texture.Region = new Rect2(x, y, SIZE, SIZE);
    }
}
