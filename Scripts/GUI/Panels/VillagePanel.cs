using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class VillagePanel : Control {
    [Export] NodePath boardPath;
    [Export] NodePath deckButtonPath;
    [Export] NodePath shopButtonPath;
    [Export] NodePath huntButtonPath;
    TabContainer board;
    public override void _Ready () {
        board = GetNode<TabContainer>(boardPath);
        GetNode<Button>(deckButtonPath).Connect("pressed", board, "set_current_tab", 0.InArray());
        GetNode<Button>(shopButtonPath).Connect("pressed", board, "set_current_tab", 1.InArray());
        GetNode<Button>(huntButtonPath).Connect("pressed", board, "set_current_tab", 2.InArray());
    }
}
