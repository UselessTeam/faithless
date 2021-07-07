using System;
using Godot;
using Utils;

public class TitleScreen : CanvasLayer {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready () {
        GetNode<Label>("Version").Text += Global.GameVersion;
        SFXHandler.Instance.Change(GameData.GameState.None);

        GetNode<AnimationPlayer>("AnimationPlayer").Connect("animation_finished",
                this, nameof(OnSplashScreenFinish));

        TitleScreenButtons genericTitleScreen = GetNode<TitleScreenButtons>("Menu/Menu buttons");
        if (FileEncoder.SaveExists()) genericTitleScreen.newGameConfirmationPath = new NodePath("../Confirmation");

        Callback.Connect(genericTitleScreen, nameof(TitleScreenButtons.NewGame), () => {
            FileEncoder.Delete();
            GameData.Instance.Deck = CardData.DefaultDeck();
        });
        Callback.Connect(genericTitleScreen, nameof(TitleScreenButtons.LoadGame), () => Global.LoadGame(GetTree()));

        GameData.Instance.Deck = CardData.All();
    }

    public void OnSplashScreenFinish (string anim) {
        SFXHandler.Instance.Change(GameData.GameState.TitleScreen);
    }
}
