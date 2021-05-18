using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class LogPanel : VBoxContainer {

    SmartText smartTextTemplate;
    HSeparator separatorTemplate;
    public override void _Ready () {
        smartTextTemplate = GetNode<SmartText>("Label");
        separatorTemplate = GetNode<HSeparator>("Separator");
    }

    public void Log (string logText) {
        SmartText label = (SmartText)smartTextTemplate.Duplicate();
        label.BbcodeText = BB.Format(logText);
        AddChild(label);
        // GD.Print("Logging :\n" + logText);
    }

    public void Separate() {
        HSeparator separator = (HSeparator)separatorTemplate.Duplicate();
        separator.Show();
        AddChild(separator);
    }
}