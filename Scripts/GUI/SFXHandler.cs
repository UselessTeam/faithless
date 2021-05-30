using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SFXHandler : AudioStreamPlayer {
    public static SFXHandler Instance;
    [Export(PropertyHint.File)] string battleMusicPath;
    [Export(PropertyHint.File)] string villageMusicPath;
    AudioStream battleMusic;
    AudioStream villageMusic;

    static Dictionary<string, AudioStreamPlayer> SoundHolder;

    public override void _Ready () {
        Instance = this;
        // GameData.Instance.Connect(nameof(GameData.ChangeGameState), this, nameof(Change));
        battleMusic = ResourceLoader.Load<AudioStream>(battleMusicPath);
        villageMusic = ResourceLoader.Load<AudioStream>(villageMusicPath);

        SoundHolder = GetChildren().Cast<AudioStreamPlayer>().ToDictionary((player) => player.Name);
    }

    public void Change (GameData.GameState state) {
        GD.Print("State: " + state.ToString());
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

    public static void PlaySFX (string Name) {
        AudioStreamPlayer player;
        if (SoundHolder.TryGetValue(Name, out player))
            player.Play();
        else
            GD.PrintErr("Could not found SFX : ", Name);
    }
}
