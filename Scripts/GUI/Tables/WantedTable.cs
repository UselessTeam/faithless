using System;
using Godot;
using Utils;

public class WantedTable : MarginContainer {
    public static WantedTable Hovered = null;
    public static WantedTable Instance () {
        return (WantedTable) ResourceLoader.Load<PackedScene>("res://Nodes/GUI/Tables/WantedTable.tscn").Instance();
    }

    [Export(PropertyHint.File)] string battleScenePath;
    [Export] NodePath namePath;
    [Export] NodePath difficultyPath;
    [Export] NodePath elementPath;
    [Export] NodePath rewardPath;
    [Export] NodePath huntPath;
    [Export] NodePath yokaiAnimationPath;
    Label nameField;
    Label difficultyField;
    Label elementField;
    RichTextLabel rewardField;
    Label huntField;

    YokaiData yokai;
    public override void _Ready () {
        nameField = GetNode<Label>(namePath);
        difficultyField = GetNode<Label>(difficultyPath);
        elementField = GetNode<Label>(elementPath);
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
        elementField.Text = yokai.Element.ToString();
        GetNode<YokaiTextures>(yokaiAnimationPath).ShowYokai(yokai.Id, false);
        rewardField.BbcodeText = $"[right]{yokai.Reward} {BB.Mon}[/right]  ";
    }

    public override void _Input (InputEvent _event) {
        if (GetFocusOwner() == this && InputHelper.IsClick(_event)) {
            GameData.Instance.yokai = yokai.Id;
            GetTree().ChangeScene(battleScenePath);
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