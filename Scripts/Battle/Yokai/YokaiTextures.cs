using System;
using System.Collections.Generic;
using Godot;

public class YokaiTextures : Node {
    private static YokaiTextures instance;
    public static YokaiTextures Instance { get { return instance; } }

    AnimatedSprite currentVisible = null;

    public override void _Ready () {
        instance = this;
    }

    public void ShowYokai (YokaiId yokai, bool withAnimation = true) {
        if (currentVisible != null) currentVisible.Visible = false;
        currentVisible = GetNode<AnimatedSprite>(yokai.ToString());

        currentVisible.Visible = true;

        if (withAnimation) currentVisible.Playing = true;
        else {
            currentVisible.Playing = false;
            currentVisible.Frame = 0;
        }
    }
    public void HideYokai (YokaiId yokai) {
        if (currentVisible == null) return;

        currentVisible.Visible = false;
        currentVisible = null;
    }

}