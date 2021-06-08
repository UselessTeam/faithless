using System;
using System.Collections.Generic;
using Godot;

public class YokaiTextures : Node {
    private static YokaiTextures instance;
    public static YokaiTextures Instance { get { return instance; } }

    public override void _Ready () {
        instance = this;
    }

    public Texture GetTexture (YokaiId yokai) {
        return GetNode<AnimatedSprite>(yokai.ToString()).Frames.GetFrame("default", 0);
    }
}