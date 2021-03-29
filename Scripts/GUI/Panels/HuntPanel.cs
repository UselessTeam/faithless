using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class HuntPanel : MarginContainer {
    [Export] NodePath containerPath;
    Control containerField;

    public override void _Ready () {
        containerField = GetNode<Control>(containerPath);
        LoadRandomYokai();
    }

    private void LoadRandomYokai () {
        Load(YokaiList);
    }

    public void Load (List<Yokai> yokais) {
        containerField.QueueFreeChildren();
        foreach (Yokai yokai in yokais) {
            WantedTable wanted = WantedTable.Instance();
            containerField.AddChild(wanted);
            wanted.Load(yokai);
        }
    }

    public static List<Yokai> YokaiList = new List<Yokai> {
        new Yokai(){
            Name = "Hitotsumekozo",
            Difficulty  = "EASY",
            DifficultyValue = 2,
            Reward = 100,
            SealSlots = 6,
        },
        new Yokai(){
            Name = "Kasa-Obake",
            Difficulty  = "MEDIUM",
            DifficultyValue = 3,
            Reward = 200,
            SealSlots = 8,
        },
        new Yokai(){
            Name = "Chochi-No-Bake",
            Difficulty  = "HARD",
            DifficultyValue = 4,
            Reward = 300,
            SealSlots = 10,

        },
        new Yokai(){
            Name = "Joro-Gumo",
            Difficulty  = "LEGENDARY",
            DifficultyValue = 5,
            Reward = 600,
            SealSlots = 12
        },
     };
}
