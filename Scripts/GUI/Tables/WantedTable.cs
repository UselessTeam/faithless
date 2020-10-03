using System;
using Godot;

public class WantedTable : MarginContainer {
    [Export] NodePath namePath;
    [Export] NodePath difficultyPath;
    [Export] NodePath weaknessPath;
    [Export] NodePath rewardPath;
    Label nameField;
    Label difficultyField;
    Label weaknessField;
    Label rewardField;
    public override void _Ready() {
        nameField = GetNode<Label>(namePath);
        difficultyField = GetNode<Label>(difficultyPath);
        weaknessField = GetNode<Label>(weaknessPath);
        rewardField = GetNode<Label>(rewardPath);
        Load(new Demon());
    }

    public void Load(Demon demon) {
        nameField.Text = demon.Name;
        difficultyField.Text = demon.Difficulty;
        weaknessField.Text = demon.Weaknesses;
        rewardField.Text = $"{demon.Reward} mon";
    }
}
