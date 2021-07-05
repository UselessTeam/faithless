using System;
using Godot;
using Utils;

public static class Global {
    public const string GameVersion = "1.1.4";

    public static void SaveGame () {
        string save = Saver.Save(GameData.Instance); // The rest of the code has to ensure that 
        save += "\n" + Saver.Save(RNG.Seed);
        FileEncoder.Write(save);
    }

    public static void LoadGame (SceneTree tree) {
        string save = "";
        try {
            if (!FileEncoder.SaveExists())
                return;
            save = FileEncoder.Read();
        } catch (Utils.WrongVersionException e) {
            ThoughtPopup.Instance.OpenCustomMessage(e.GetMessage() + "\n\n[url=~title]Back to the title screen[/url]");
            var callback = Callback.ConnectOnce(ThoughtPopup.Instance, "popup_hide", () => tree.ChangeScene("res://Scenes/TitleScreen.tscn"));
            return;
        }
        var saveItems = Loader.LoadMany(save);
        GameData.Instance = (GameData) saveItems[0];
        RNG.StartCycle((int) saveItems[1]);
    }

    public static void ResetGame (SceneTree tree) {
        var oldInstance = GameData.Instance;
        GameData.Instance = null;

        GameData newData = (GameData) ((PackedScene) GD.Load("res://Nodes/GameData.tscn")).Instance();
        tree.Root.AddChild(newData);
    }
}