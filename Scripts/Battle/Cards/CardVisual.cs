using System;
using Godot;

public class CardVisual : Control {
    [Signal] public delegate void OnClick (byte id);
    public CardId Card { get; private set; }
    [Export] NodePath backgroundPath;
    [Export] NodePath namePath;
    [Export] NodePath costPath;
    [Export] NodePath kanjiPath;
    TextureRect backgroundField;
    Label nameField;
    Label costField;
    Label kanjiField;
    public Control Holder;

    public bool IsDisabled { get; set; } = false;

    public Tween MyTween {
        get {
            Tween tween = GetNodeOrNull<Tween>("Tween");
            if (tween == null) {
                tween = new Tween();
                tween.Name = "Tween";
                AddChild(tween);
            }
            return tween;
        }
    }

    public static CardVisual Instance () {
        return (CardVisual) ResourceLoader.Load<PackedScene>("res://Nodes/Battle/CardVisual.tscn").Instance();
    }

    public override void _Ready () {
        Holder = GetNode<Control>("Holder");
        backgroundField = GetNode<TextureRect>(backgroundPath);
        nameField = GetNode<Label>(namePath);
        costField = GetNode<Label>(costPath);
        kanjiField = GetNode<Label>(kanjiPath);
        Connect("mouse_entered", this, nameof(MouseEntered));
        Connect("mouse_exited", this, nameof(MouseExited));
        Connect("focus_entered", this, nameof(FocusEnter));
        Connect("focus_exited", this, nameof(FocusExit));
        MyTween.Start();
    }

    public void ShowCard (CardData card) {
        Card = card.Id;
        backgroundField.Texture = card.Texture;
        nameField.Text = card.Name;
        costField.Text = card.Cost.ToString();
        kanjiField.Text = card.Kanji;
        Show();
    }
    public void Disappear (float split = 0f) {
        MyTween.InterpolateProperty(this, "modulate:a", 1, 0, 0.5f);
        MyTween.InterpolateProperty(this, "rect_min_size:x", RectMinSize.x, 0 - split, 0.4f, Tween.TransitionType.Cubic, Tween.EaseType.InOut, 0.1f);
        MyTween.InterpolateProperty(Holder, "rect_position", Holder.RectPosition, new Vector2(Holder.RectPosition.x, -120), 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        MyTween.Start();
    }
    public void MoveFrom (Vector2 position) {
        MyTween.InterpolateProperty(this, "modulate:a", 0, 1, 0.5f);
        MyTween.InterpolateProperty(Holder, "rect_position", position, Holder.RectPosition, 0.5f);
        MyTween.Start();
    }
    public void Pull (float offset) {
        MyTween.InterpolateProperty(Holder, "rect_position", Holder.RectPosition, new Vector2(Holder.RectPosition.x, offset), 0.2f, Tween.TransitionType.Back, Tween.EaseType.Out);
        MyTween.Start();
    }

    [Signal] public delegate void FocusEntered (CardId id);
    [Signal] public delegate void FocusExited (CardId id);

    private void MouseEntered () {
        if (IsDisabled) return;
        GrabFocus();
    }
    private void MouseExited () {
        if (IsDisabled) return;
        ReleaseFocus();
    }
    private void FocusEnter () {
        if (IsDisabled) return;
        Modulate = new Color(1.2f, 1.2f, 1.2f, 1f);
        EmitSignal(nameof(FocusEntered), Card);
    }
    private void FocusExit () {
        if (IsDisabled) return;
        Modulate = Colors.White;
        EmitSignal(nameof(FocusExited), Card);
    }
    public override void _GuiInput (InputEvent _event) {
        if (IsDisabled) return;
        if (InputHelper.IsClick(_event)) {
            EmitSignal(nameof(OnClick), GetIndex());
        }
    }
}