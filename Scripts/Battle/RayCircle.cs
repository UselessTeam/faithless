using System;
using System.Collections.Generic;
using Godot;

public class RayCircle : ColorRect {
    GradientTexture gradientTexture = new GradientTexture();
    int N = 2;

    static readonly Color NO_COLOR = new Color(0, 0, 0, 0);
    static readonly Color[] elementColors = new Color[] {
        NO_COLOR,
        Colors.Red,
        Colors.Blue,
        Colors.Green,
        Colors.Orange,
        Colors.Gray,
    };
    public override void _Ready () {
        gradientTexture.Gradient = new Gradient();
        GD.Print(((ShaderMaterial) Material).GetPropertyList());
        ((ShaderMaterial) Material).SetShaderParam("colors", gradientTexture);
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
        Color color = elementColors[(int) e];
        gradientTexture.Gradient.SetColor(i, color);
        if (i == 0) {
            gradientTexture.Gradient.SetColor(N, color);
        }
    }
}
