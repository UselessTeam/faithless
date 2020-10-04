using System;
using System.Collections.Generic;
using Godot;

public class SealSlot : Control {
    [Signal] delegate void OnClick (byte id);
    public int id;

    public override void _Ready () {
        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnSealSlot));
    }

    public void ShowSlot (Element element) {
        var texture = GetNode<TextureRect>("Sprite").Texture as AtlasTexture;
        var region = texture.Region;
        region.Position = element switch
        {
            Element.Fire => new Vector2(0, 400),
            Element.Water => new Vector2(200, 400),
            Element.Earth => new Vector2(400, 400),
            Element.Metal => new Vector2(0, 600),
            Element.Wood => new Vector2(200, 600),
            _ => new Vector2(400, 600)
        };
        texture.Region = region;
        Show();
    }

    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton)
            EmitSignal(nameof(OnClick), id);
    }

}