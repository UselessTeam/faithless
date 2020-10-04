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

    public bool IsDisabled { get; set; } = false;

    public Tween MyTween { get { return GetNode<Tween>("Tween"); } }

    public static CardVisual Instance () {
        return (CardVisual) ResourceLoader.Load<PackedScene>("res://Nodes/Battle/CardVisual.tscn").Instance();
    }

    public override void _Ready () {
        backgroundField = GetNode<TextureRect>(backgroundPath);
        nameField = GetNode<Label>(namePath);
        kanjiField = GetNode<Label>(kanjiPath);
        MyTween.Start();
    }

    public void ShowCard (Card card) {
        backgroundField.Texture = card.Texture;
        nameField.Text = card.Name;
        kanjiField.Text = card.Kanji;
        Show();
    }

    public void Disappear () {
        MyTween.InterpolateProperty(this, "modulate:a", 1, 0, 0.5f);
        MyTween.Start();
    }
    public void MoveFrom (Vector2 position) {
        MyTween.InterpolateProperty(this, "modulate:a", 0, 1, 0.5f);
        MyTween.InterpolateProperty(this, "rect_position", position, RectPosition + new Vector2(180, 0) * GetIndex(), 0.5f);
        MyTween.Start();
    }

    public override void _GuiInput (InputEvent _event) {
        if (IsDisabled) return;
        if (_event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int) ButtonList.Left && mouseEvent.Pressed) {
            EmitSignal(nameof(OnClick), GetIndex());
        }
    }
}