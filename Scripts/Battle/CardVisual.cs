using System;
using Godot;

public class CardVisual : Control {
    [Signal] public delegate void OnClick (byte id);
    public CardId Card { get; private set; }
    [Export] NodePath backgroundPath;
    [Export] NodePath namePath;
    [Export] NodePath kanjiPath;
    TextureRect backgroundField;
    Label nameField;
    Label kanjiField;
    public Control Holder;

    public bool IsDisabled { get; set; } = false;

    public Tween MyTween { get { return GetNode<Tween>("Tween"); } }

    public static CardVisual Instance () {
        return (CardVisual) ResourceLoader.Load<PackedScene>("res://Nodes/Battle/CardVisual.tscn").Instance();
    }

    public override void _Ready () {
        Holder = GetNode<Control>("Holder");
        backgroundField = GetNode<TextureRect>(backgroundPath);
        nameField = GetNode<Label>(namePath);
        kanjiField = GetNode<Label>(kanjiPath);
        Connect("mouse_entered", this, nameof(MouseEntered));
        Connect("mouse_exited", this, nameof(MouseExited));
        Connect("focus_entered", this, nameof(FocusEnter));
        Connect("focus_exited", this, nameof(FocusExit));
        MyTween.Start();
    }

    public void ShowCard (Card card) {
        Card = card.Id;
        backgroundField.Texture = card.Texture;
        nameField.Text = card.Name;
        kanjiField.Text = card.Kanji;
        Show();
    }

    public void Reset () {
        Holder.RectPosition = Vector2.Zero;
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

    [Signal] public delegate void FocusEntered (CardId id);
    [Signal] public delegate void FocusExited (CardId id);

    private void MouseEntered () {
        GrabFocus();
    }
    private void MouseExited () {
        ReleaseFocus();
    }
    private void FocusEnter () {
        Modulate = new Color(1.2f, 1.2f, 1.2f, 1f);
        EmitSignal(nameof(FocusEntered), Card);
    }
    private void FocusExit () {
        Modulate = Colors.White;
        EmitSignal(nameof(FocusExited), Card);
    }
    public override void _GuiInput (InputEvent _event) {
        if (IsDisabled) return;
        if (_event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int) ButtonList.Left && mouseEvent.Pressed) {
            EmitSignal(nameof(OnClick), GetIndex());
        }
    }
}