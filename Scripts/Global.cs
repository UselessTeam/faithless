using System;
using Godot;
using Utils;

public static class Global {
    public const string GameVersion = "1.1.3";

    public static void SaveGame () {
        string save = Utils.Saver.SaveSingle(GameData.Instance);
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
            var callback = Callback.Connect(ThoughtPopup.Instance, "popup_hide", () => tree.ChangeScene("res://Scenes/TitleScreen.tscn"));
            callback.CallOnce = true;
            return;
        }
        Loader load = new Loader();
        GameData.Instance = (GameData) load.FromData(save);
    }

    public static void ResetGame (SceneTree tree) {
        var oldInstance = GameData.Instance;
        GameData.Instance = null;

        GameData newData = (GameData) ((PackedScene) GD.Load("res://Nodes/GameData.tscn")).Instance();
        tree.Root.AddChild(newData);
    }
}