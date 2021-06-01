using System;
using Godot;

public class TitleScreen : CanvasLayer {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready () {
        GetNode<Label>("Version").Text += Global.GameVersion;
        SFXHandler.Instance.Change(GameData.GameState.None);

        GetNode<AnimationPlayer>("AnimationPlayer").Connect("animation_finished",
                this, nameof(OnSplashScreenFinish));
    }

    public void OnSplashScreenFinish (string anim) {
        SFXHandler.Instance.Change(GameData.GameState.TitleScreen);
    }
}
