using System;
using System.Collections.Generic;
using Godot;

public class Demon : Resource {
    [Export] public string Name = "[OBAKE]";
    [Export] public string Difficulty = "[DIFFICULTY]";
    [Export] public int DifficultyValue = 2;
    [Export] public int Reward = 300;
    [Export] public string Weaknesses = "[WEAKNESSES]";
    [Export] public int SealSlots = 6;

    public bool CheckWinCondition () {
        return true;
    }
}