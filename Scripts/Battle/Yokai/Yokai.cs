using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public enum YokaiId {
    None,
    Hitotsumekozo,
    Bakezori,
    Kasaobake,
    Chochinobake,
    Sadako,
    Jorogumo,
    TOTAL,
}

public static class YokaiIdExtension {
    public static YokaiData Data (this YokaiId yokai) => YokaiData.YokaiList[yokai];
}

public enum Pattern { Default, Rotary, Vertical }

public class YokaiData {
    public YokaiId Id = YokaiId.None;
    public string Name = "[OBAKE]";
    public int Level = 0;
    public int Reward = 300;
    public Element Element = Element.None;

    public int SealSlots = 6;
    public Pattern Pattern = Pattern.Default;

    public string Difficulty => Level switch {
        0 => "Defenceless",
        1 => "Easy",
        2 => "Medium",
        3 => "Hard",
        4 => "Legendary",
        _ => "Unknown",
    };

    public static Dictionary<YokaiId, YokaiData> YokaiList = (new List<YokaiData> {
        null,
        new YokaiData () {
            Id = YokaiId.Hitotsumekozo,
                Name = "Hitotsume kozo",
                Level = 1,
                Reward = 100,
                SealSlots = 6,
        },
        new YokaiData () {
            Id = YokaiId.Bakezori,
                Name = "Bakezori",
                Level = 1,
                Element = Element.Earth,
                Reward = 150,
                SealSlots = 8,
        },
        new YokaiData () {
            Id = YokaiId.Kasaobake,
                Name = "Kasa Obake",
                Level = 2,
                Reward = 250,
                SealSlots = 8,
                Pattern = Pattern.Rotary,
        },
        new YokaiData () {
            Id = YokaiId.Chochinobake,
                Name = "Chochi no Bake",
                Level = 3,
                Element = Element.Fire,
                Reward = 500,
                SealSlots = 10,
        },
        new YokaiData () {
            Id = YokaiId.Sadako,
                Name = "Sadako chan",
                Level = 3,
                Element = Element.Water,
                Reward = 650,
                SealSlots = 10,
                Pattern = Pattern.Rotary,
        },
        new YokaiData () {
            Id = YokaiId.Jorogumo,
                Name = "Jorogumo",
                Level = 4,
                Element = Element.Wood,
                Reward = 1000,
                SealSlots = 12,
                Pattern = Pattern.Rotary,
        },
    }).ToDictionary((yokai) => yokai?.Id ?? YokaiId.None);
}