using System;
using System.Collections.Generic;
using Godot;

public class SealSlot : Control {
    [Signal] delegate void OnClick (int id);
    public SealingCircle Circle;
    public TextureRect Glow;
    public int id;

    public Vector2 initialPosition;

    public override void _Ready () {
        Connect("mouse_entered", this, nameof(MouseEntered));
        Connect("mouse_exited", this, nameof(MouseExited));
        Connect("focus_entered", this, nameof(FocusEnter));
        Connect("focus_exited", this, nameof(FocusExit));

        Glow = GetNode<TextureRect>("Glow");

        Connect(nameof(OnClick), BattleScene.Instance, nameof(BattleScene.ClickOnTarget));
        initialPosition = MySprite.RectPosition;
    }
    public Tween MyTween { get { return GetNode<Tween>("Tween"); } }
    public TextureRect MySprite { get { return GetNode<TextureRect>("Sprite"); } }

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

    public void StartGlow () {
        Glow.Show();
    }
    public void StopGlow () {
        Glow.Hide();
    }

    public override void _GuiInput (InputEvent _event) {
        base._GuiInput(_event);
        if (InputHelper.IsClick(_event))
            EmitSignal(nameof(OnClick), id);
    }

    private void MouseEntered () {
        GrabFocus();
    }
    private void MouseExited () {
        ReleaseFocus();
    }

    Color keptColor;
    private void FocusEnter () {
        keptColor = Modulate;
        Color m = keptColor;
        m.r *= 1.2f;
        m.g *= 1.2f;
        m.b *= 1.2f;
        Modulate = m;
        BattleScene.Instance.DescribeSeal(BattleScene.SealSlots[id]);
    }
    private void FocusExit () {
        Modulate = keptColor;
        BattleScene.Instance.DescribeDefault();
    }

}