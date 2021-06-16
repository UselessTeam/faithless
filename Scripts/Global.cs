using System;
using Godot;
using Utils;

public static class Global {
    public const string GameVersion = "1.1.4";

    public static void SaveGame () {
        string save = Saver.Save(GameData.Instance);
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
        GameData.Instance = (GameData) Loader.Load(save);
    }

    public static void ResetGame (SceneTree tree) {
        var oldInstance = GameData.Instance;
        GameData.Instance = null;

        GameData newData = (GameData) ((PackedScene) GD.Load("res://Nodes/GameData.tscn")).Instance();
        tree.Root.AddChild(newData);
    }
}