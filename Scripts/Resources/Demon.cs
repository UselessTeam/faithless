using System;
using System.Collections.Generic;
using Godot;

public class Demon : Resource {
    [Export] public string Name = "[OBAKE]";
    [Export] public string Difficulty = "[DIFFICULTY]";
    [Export] public int Reward = 999;
    [Export] public string Weaknesses = "[WEAKNESSES]";
    [Export] public readonly int SealSlots = 2;
}