using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class HuntPanel : MarginContainer {
    [Export] NodePath containerPath;
    Control containerField;

    public override void _Ready () {
        containerField = GetNode<Control>(containerPath);
        LoadRandomDemons();
    }

    private void LoadRandomDemons () {
        Load(DemonList);
    }

    public void Load (List<Demon> demons) {
        containerField.QueueFreeChildren();
        foreach (Demon demon in demons) {
            WantedTable wanted = WantedTable.Instance();
            containerField.AddChild(wanted);
            wanted.Load(demon);
        }
    }

    public static List<Demon> DemonList = new List<Demon> {
        new Demon(){
            Name = "Hitotsumekozo",
            Difficulty  = "EASY",
            DifficultyValue = 2,
            Reward = 100,
            SealSlots = 6,
        },
        new Demon(){
            Name = "Kasa-Obake",
            Difficulty  = "MEDIUM",
            DifficultyValue = 3,
            Reward = 200,
            SealSlots = 8,
        },
        new Demon(){
            Name = "Chochi-No-Bake",
            Difficulty  = "HARD",
            DifficultyValue = 4,
            Reward = 300,
            SealSlots = 10,

        },
        new Demon(){
            Name = "Joro-Gumo",
            Difficulty  = "LEGENDARY",
            DifficultyValue = 5,
            Reward = 600,
            SealSlots = 12
        },
     };
}
