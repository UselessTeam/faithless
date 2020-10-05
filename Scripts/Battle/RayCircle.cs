using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class RayCircle : ColorRect {
    Tween tween;
    GradientTexture gradientTexture = new GradientTexture();
    int N = 2;

    private static Color C (int r, int g, int b) {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
    public static readonly Color NO_COLOR = new Color(0, 0, 0, 0);
    public static readonly Dictionary<Element, Color> ELEMENT_COLORS = new Dictionary<Element, Color> {
        {Element.None, NO_COLOR},
        {Element.Fire, C(189,89,72)},
        {Element.Water, C(102,206,232)},
        {Element.Earth, C(232,212,168)},
        {Element.Metal, C(177,210,227)},
        {Element.Wood, C(190,227,214)},
    };
    public override void _Ready () {
        gradientTexture.Gradient = new Gradient();
        ((ShaderMaterial) Material).SetShaderParam("colors", gradientTexture);
        tween = new Tween();
        AddChild(tween);
    }
    public void SetSlotCount (int n) {
        while (N > n) {
            GD.Print(gradientTexture.Gradient.GetPointCount());
            gradientTexture.Gradient.RemovePoint(0);
            N--;
        }
        N = n;
        ((ShaderMaterial) Material).SetShaderParam("N", N);
        for (int index = 0 ; index <= n ; index++) {
            float frac = (float) index / N;
            if (index < gradientTexture.Gradient.GetPointCount()) {
                gradientTexture.Gradient.SetOffset(index, frac);
                gradientTexture.Gradient.SetColor(index, NO_COLOR);
            } else {
                gradientTexture.Gradient.AddPoint(frac, NO_COLOR);
            }
        }
    }

    public void SetSlot (Element e, int i) {
        SetSlotColor(i, ELEMENT_COLORS[e]);
    }

    public void SetSlotColor (int i, Color color) {
        gradientTexture.Gradient.SetColor(i, color);
        if (i == 0) {
            gradientTexture.Gradient.SetColor(N, color);
        }
    }

    public async Task Seal () {
        tween.InterpolateProperty(Material, "shader_param/seal", 0f, 1f, 2f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
        SignalAwaiter awaiter = ToSignal(tween, "tween_all_completed");
        tween.Start();
        await awaiter;
    }
}
