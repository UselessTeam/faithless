using System;
using Godot;

public class CardVisual : MarginContainer {
    [Signal] public delegate void OnClick (byte id);

    [Export] NodePath backgroundPath;
    [Export] NodePath namePath;
    [Export] NodePath kanjiPath;
    TextureRect backgroundField;
    Label nameField;
    Label kanjiField;

    public override void _Ready () {
        backgroundField = GetNode<TextureRect>(backgroundPath);
        nameField = GetNode<Label>(namePath);
        kanjiField = GetNode<Label>(kanjiPath);
    }

    public void ShowCard (Card card) {
        backgroundField.Texture = card.Texture;
        nameField.Text = card.Name;
        kanjiField.Text = card.Kanji;
        Show();
    }

    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton) {
            EmitSignal(nameof(OnClick), GetIndex());
        }
    }
}