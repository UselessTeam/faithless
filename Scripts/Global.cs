using System;
using Godot;
using Utils;

public static class Global {
    public const string GameVersion = "1.1.3";

    public static void SaveGame () {
        string save = Utils.Saver.SaveSingle(GameData.Instance);
        FileEncoder.Write(save);
    }
}