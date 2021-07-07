using System;
using Godot;

public class BattleMessages : CanvasLayer {
    static BattleMessages instance;

    Control discardMessage;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready () {
        discardMessage = GetNode<Control>("DiscardMessage");
        instance = this;
    }

    public static void OpenDiscardMessage () {
        instance.discardMessage.Show();
    }
    public static void CloseDiscardMessage () {
        instance.discardMessage.Hide();
    }


    public static void EndOfBattle () { //Hide this layer
        instance.Layer = 0;
    }
}
