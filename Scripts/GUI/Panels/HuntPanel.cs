using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class HuntPanel : MarginContainer {
    [Export] NodePath containerPath;
    Control containerField;

    public override void _Ready () {
        containerField = GetNode<Control>(containerPath);
        Load(new List<Demon> {
            new Demon(), new Demon(), new Demon(), new Demon(), new Demon()
        });
    }

    public void Load (List<Demon> demons) {
        containerField.QueueFreeChildren();
        foreach (Demon demon in demons) {
            WantedTable wanted = WantedTable.Instance();
            containerField.AddChild(wanted);
            wanted.Load(demon);
        }
    }
}
