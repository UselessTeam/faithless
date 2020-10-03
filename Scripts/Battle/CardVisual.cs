using System;
using Godot;

public class CardVisual : Control {
    [Signal] public delegate void OnClick (byte id);
    public byte id;

    public override void _Ready () {
        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnCard));
    }

    public void ShowCard (CardId card) {
        GetNode<TextureRect>(nameof(TextureRect)).Texture = CardTextures.Instance.GetTexture(card);
        GetNode<TextureRect>(nameof(TextureRect)).Show();
        //TODO (Show kanji and name)
    }

    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton)
            EmitSignal(nameof(OnClick), id);
    }
}