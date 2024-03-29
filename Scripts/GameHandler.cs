using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Utils;

public class GameHandler : Node {
    public static GameHandler Instance;

    [Signal] public delegate void InstanceLoaded (int value);

    public override void _Ready () {
        FileEncoder.CurrentVersion = Global.GameVersion;
        FileEncoder.IsSaveCompatible = (Version v) => v >= Global.OldestSaveCompatibleVersion;

        if (Instance != null)
            GD.PrintErr("Error: GameHandler already has an instance");
        else
            Instance = this;
    }

}