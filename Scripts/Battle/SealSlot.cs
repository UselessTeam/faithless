using System;
using System.Collections.Generic;
using Godot;

public class SealSlot : Control {
    [Signal] delegate void OnClick (byte id);
    public SealingCircle Circle;
    public int id;

    public override void _Ready () {
        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnSealSlot));
    }
    public Tween MyTween { get { return GetNode<Tween>("Tween"); } }
    public TextureRect MySprite { get { return GetNode<TextureRect>("Sprite"); } }

    public void ShowSlot (Element element) {
        var texture = MySprite.Texture as AtlasTexture;
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

    Color color = RayCircle.NO_COLOR;
    public void ModifyColor (Color color) {
        Circle.RayCircle.SetSlotColor(id, color);
        Color modulate = MySprite.Modulate;
        modulate.a = color.a;
        MySprite.Modulate = modulate;
    }
    public void Change (Element e) {
        MyTween.InterpolateMethod(this, nameof(ModifyColor), color, RayCircle.ELEMENT_COLORS[(int) e], 0.5f);
        MyTween.Start();
    }
    public void MoveTo (Vector2 position) {
        MyTween.InterpolateMethod(GetNode<TextureRect>("Sprite"), "rect_position", RectPosition, position, 0.5f);
        MyTween.Start();
    }


    public override void _GuiInput (InputEvent _event) {
        if (_event is InputEventMouseButton)
            EmitSignal(nameof(OnClick), id);
    }

}