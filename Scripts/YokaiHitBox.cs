using System;
using Godot;

public class YokaiHitBox : Control {
	[Signal] public delegate void OnClick (int id);
	private TextureRect glow;

	public override void _Ready () {
		// Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnTarget));
		glow = GetNode<TextureRect>("Glow");
		BattleScene.yokaiHitBoxPath = GetPath();
		// GD.Print(this.GetPath());
	}

	public void StartGlow () {
		glow.Show();
	}
	public void StopGlow () {
		glow.Hide();
	}

	public override void _GuiInput (InputEvent _event) {
		if (InputHelper.IsClick(_event))
			EmitSignal(nameof(OnClick), -1);
	}
}
