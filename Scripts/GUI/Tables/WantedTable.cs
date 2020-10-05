using System;
using Godot;
using Utils;

public class WantedTable : MarginContainer {
    public static WantedTable Hovered = null;
    public static WantedTable Instance () {
        return (WantedTable) ResourceLoader.Load<PackedScene>("res://Nodes/GUI/Tables/WantedTable.tscn").Instance();
    }

    [Export(PropertyHint.File)] string combatScenePath;
    [Export] NodePath namePath;
    [Export] NodePath difficultyPath;
    [Export] NodePath weaknessPath;
    [Export] NodePath rewardPath;
    [Export] NodePath huntPath;
    [Export] NodePath imagePath;
    Label nameField;
    Label difficultyField;
    Label weaknessField;
    RichTextLabel rewardField;
    Label huntField;

    Demon demon;
    public override void _Ready () {
        nameField = GetNode<Label>(namePath);
        difficultyField = GetNode<Label>(difficultyPath);
        weaknessField = GetNode<Label>(weaknessPath);
        rewardField = GetNode<RichTextLabel>(rewardPath);
        huntField = GetNode<Label>(huntPath);
        Connect("mouse_entered", this, nameof(MouseEntered));
        Connect("mouse_exited", this, nameof(MouseExited));
        Connect("focus_entered", this, nameof(FocusEntered));
        Connect("focus_exited", this, nameof(FocusExited));
    }

    public void Load (Demon _demon) {
        demon = _demon;
        nameField.Text = demon.Name;
        difficultyField.Text = demon.Difficulty;
        weaknessField.Text = demon.Weaknesses;
        GetNode<TextureRect>(imagePath).Texture = CardTextures.Instance.GetNode<Node2D>("Demons").GetNode<Sprite>(demon.Name).Texture;
        rewardField.BbcodeText = $"[right]{demon.Reward} {BB.Mon}[/right]  ";
    }

    public override void _Input (InputEvent @event) {
        if (GetFocusOwner() == this && @event is InputEventMouseButton eventMouse && eventMouse.ButtonIndex == (int) ButtonList.Left && eventMouse.Pressed) {
            GameData.Instance.Oni = demon;
            GetTree().ChangeScene(combatScenePath);
        }
    }

    private void MouseEntered () {
        GrabFocus();
    }
    private void MouseExited () {
        ReleaseFocus();
    }
    private void FocusEntered () {
        huntField.AddColorOverride("font_color", Colors.Red);
    }
    private void FocusExited () {
        huntField.AddColorOverride("font_color", Colors.Black);
    }
}