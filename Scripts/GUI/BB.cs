using System;
using Godot;

public static class BB {

    public static readonly string Mon = "[img]res://Assets/Sprites/GUI/mon.png[/img]";
    public static readonly string Ki = "[img=24]res://Assets/Sprites/Combat/Talismans/ki.png[/img]";
    public static readonly string FireSeal = "[img=24]res://Assets/Sprites/GUI/icons/fire_seal.png[/img]";
    public static readonly string WaterSeal = "[img=24]res://Assets/Sprites/GUI/icons/water_seal.png[/img]";
    public static readonly string WoodSeal = "[img=24]res://Assets/Sprites/GUI/icons/wood_seal.png[/img]";
    public static readonly string MetalSeal = "[img=24]res://Assets/Sprites/GUI/icons/metal_seal.png[/img]";
    public static readonly string EarthSeal = "[img=24]res://Assets/Sprites/GUI/icons/earth_seal.png[/img]";
    public static readonly string EmptySeal = "[img=24]res://Assets/Sprites/GUI/icons/empty_seal.png[/img]";

    public static string Format (string value) {
        return value
            .Replace("[?", "[url=?")
            .Replace("[/?]", "[/url]")
            .Replace("[url=", "[color=#4ff9f9][url=")
            .Replace("[/url]", "[/url][/color]")
            .Replace("[kanji]", "[font=res://Assets/Theme/Fonts/riiltf_22.tres]")
            .Replace("[/kanji]", "[/font]")
            .Replace("[mon]", BB.Mon)
            .Replace("[ki]", BB.Ki)
            .Replace("[fire-seal]", BB.FireSeal)
            .Replace("[water-seal]", BB.WaterSeal)
            .Replace("[wood-seal]", BB.WoodSeal)
            .Replace("[metal-seal]", BB.MetalSeal)
            .Replace("[earth-seal]", BB.EarthSeal)
            .Replace("[empty-seal]", BB.EmptySeal);
    }
}