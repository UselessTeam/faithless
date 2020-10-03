using System;
using System.Collections.Generic;
using Godot;

public class BattleScene : Node2D {
    [Export] Demon currentDemon = new Demon ();

    public override void _Ready () {
        GetNode<SealingCircle> ("SealingCircle").ShowSlots (2);
    }
}