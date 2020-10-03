using System;
using System.Collections.Generic;
using Godot;

public class CardTextures : Node {
    private static CardTextures instance;
    public static CardTextures Instance { get { return instance; } }

    [Export] Texture FireTexture;
    [Export] Texture WaterTexture;
    [Export] Texture WoodTexture;
    [Export] Texture EarthTexture;
    [Export] Texture MetalTexture;
    [Export] Texture ColorlessTexture;

    public override void _Ready () {
        instance = this;
    }

    public Texture GetTexture (Element element) {
        return element switch
        {
            Element.Fire => FireTexture,
            Element.Water => WaterTexture,
            Element.Wood => WoodTexture,
            Element.Earth => EarthTexture,
            Element.Metal => MetalTexture,
            _ => ColorlessTexture,
        };
    }
}