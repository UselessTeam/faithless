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

    public Texture GetTexture (CardId card) {
        return (card == CardId.BasicFire) ? FireTexture :
        (card == CardId.BasicWater) ? WaterTexture :
        (card == CardId.BasicEarth) ? EarthTexture :
        (card == CardId.BasicWood) ? WoodTexture :
        (card == CardId.BasicMetal) ? MetalTexture : null;
    }
}