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

    YokaiData yokai;
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

    public void Load (YokaiData _yokai) {
        yokai = _yokai;
        nameField.Text = yokai.Name;
        difficultyField.Text = yokai.Difficulty;
        weaknessField.Text = yokai.Weaknesses;
        GetNode<TextureRect>(imagePath).Texture = YokaiTextures.Instance.GetTexture(yokai.Id);
        rewardField.BbcodeText = $"[right]{yokai.Reward} {BB.Mon}[/right]  ";
    }

    public override void _Input (InputEvent _event) {
        if (GetFocusOwner() == this && InputHelper.IsClick(_event)) {
            GameData.Instance.yokai = yokai.Id;
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