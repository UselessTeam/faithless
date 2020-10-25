using System;
using System.Collections.Generic;
using Godot;

public class SealSlot : Control {
    [Signal] delegate void OnClick (int id);
    public SealingCircle Circle;
    public int id;

    public Vector2 initialPosition;

    public override void _Ready () {
        // MyBackSprite.Set("z", -1);
        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnTarget));
        initialPosition = MySprite.RectPosition;
    }
    public Tween MyTween { get { return GetNode<Tween>("Tween"); } }
    public TextureRect MySprite { get { return GetNode<TextureRect>("Sprite"); } }
    // public TextureRect MyBackSprite { get { return GetNode<TextureRect>("Node2D/BackSprite"); } }

    public void ShowSlot (Element element) {
        var texture = MySprite.Texture as AtlasTexture;
        var region = texture.Region;
        region.Position = element switch {
            Element.Fire => new Vector2(0, 400),
            Element.Water => new Vector2(200, 400),
            Element.Earth => new Vector2(400, 400),
            Element.Metal => new Vector2(0, 600),
            Element.Wood => new Vector2(200, 600),
            _ => new Vector2(400, 600)
        };
        texture.Region = region;
        MySprite.RectPosition = initialPosition;
        MySprite.Modulate = new Color(1, 1, 1, 1);
        Show();
    }

    Color color = RayCircle.NO_COLOR;
    public void ModifyColor (Color color) {
        Circle.RayCircle.SetSlotColor(id, color);
        Color modulate = MySprite.Modulate;
        modulate.a = color.a;
        MySprite.Modulate = modulate;
        // GD.Print("alpha : ", MyBackSprite.Modulate.a);
    }
    public void ModifySpritePosition (Vector2 position) {
        MySprite.RectPosition = position;
        MySprite.Show();
    }
    public void Change (Element e) {
        MyTween.InterpolateMethod(this, nameof(ModifyColor), color, RayCircle.ELEMENT_COLORS[e], 0.5f);
        MyTween.Start();
    }
    public void MoveTo (Vector2 vector) { // User should use ShowSlot after this method is done to replace the sprite 
        MyTween.InterpolateMethod(this, "ModifySpritePosition", initialPosition, initialPosition + vector, 0.5f);
        MyTween.Start();
    }


    public override void _GuiInput (InputEvent _event) {
        if (InputHelper.IsClick(_event))
            EmitSignal(nameof(OnClick), id);
    }

}