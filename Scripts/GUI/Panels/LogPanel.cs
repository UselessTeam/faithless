using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class LogPanel : VBoxContainer {
    public override void _Ready () {

    }

    public void Log (string logText) {
        var textItem = new Label();
        textItem.Text = BB.Format(logText);
        AddChild(textItem);
        GD.Print("Logging :\n" + logText);
    }
}