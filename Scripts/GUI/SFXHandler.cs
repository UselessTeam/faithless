using System;
using Godot;

public class SFXHandler : AudioStreamPlayer {
    public static SFXHandler Instance;
    [Export(PropertyHint.File)] string battleMusicPath;
    [Export(PropertyHint.File)] string villageMusicPath;
    AudioStream battleMusic;
    AudioStream villageMusic;
    public override void _Ready () {
        Instance = this;
        GameData.Instance.Connect(nameof(GameData.ChangeGameState), this, nameof(Change));
        battleMusic = ResourceLoader.Load<AudioStream>(battleMusicPath);
        villageMusic = ResourceLoader.Load<AudioStream>(villageMusicPath);
    }

    public void Change (GameData.GameState state) {
        switch (state) {
            case GameData.GameState.Battle:
                Stream = battleMusic;
                Play();
                Playing = true;
                break;
            case GameData.GameState.Village:
                Stream = villageMusic;
                Play();
                Playing = true;
                break;
        }
    }
}
