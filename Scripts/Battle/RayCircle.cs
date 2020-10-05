using System;
using System.Collections.Generic;
using Godot;

public class RayCircle : ColorRect {
    List<Tween> tweens = new List<Tween>();
    GradientTexture gradientTexture = new GradientTexture();
    int N = 2;

    public static readonly Color NO_COLOR = new Color(0, 0, 0, 0);
    public static readonly Color[] ELEMENT_COLORS = new Color[] {
        NO_COLOR,
        Colors.Red,
        Colors.Blue,
        Colors.Green,
        Colors.Orange,
        Colors.Gray,
    };
    public override void _Ready () {
        gradientTexture.Gradient = new Gradient();
        ((ShaderMaterial) Material).SetShaderParam("colors", gradientTexture);
        AddTween();
        AddTween();
    }
    private void AddTween () {
        Tween tween = new Tween();
        AddChild(tween);
        tweens.Add(tween);
    }
    public void SetSlotCount (int n) {
        while (N > n) {
            GD.Print(gradientTexture.Gradient.GetPointCount());
            gradientTexture.Gradient.RemovePoint(0);
            tweens[0].QueueFree();
            tweens.RemoveAt(0);
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
                AddTween();
            }
        }
    }

    public void SetSlot (Element e, int i) {
        SetSlotColor(i, ELEMENT_COLORS[(int) e]);
    }

    public void TweenSlot (Element e, int i) {
        Color color = ELEMENT_COLORS[(int) e];
        tweens[i].InterpolateMethod(this, nameof(SetSlotColor), color, color, 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
    }

    public void SetSlotColor (int i, Color color) {
        gradientTexture.Gradient.SetColor(i, color);
        if (i == 0) {
            gradientTexture.Gradient.SetColor(N, color);
        }
    }
}
