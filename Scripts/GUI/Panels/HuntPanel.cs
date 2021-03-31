using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class HuntPanel : MarginContainer {
    [Export] NodePath containerPath;
    Control containerField;

    public override void _Ready () {
        containerField = GetNode<Control>(containerPath);
        LoadAllYokai();
    }

    private void LoadAllYokai () {
        containerField.QueueFreeChildren();
        for (YokaiId yokai = YokaiId.Hitotsumekozo ; yokai != YokaiId.TOTAL ; yokai++) {
            WantedTable wanted = WantedTable.Instance();
            containerField.AddChild(wanted);
            wanted.Load(yokai.Data());
        }
    }


}
