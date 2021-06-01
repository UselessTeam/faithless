using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;


public class YokaiData {
    public YokaiId Id = YokaiId.None;
    public string Name = "[OBAKE]";
    public string Difficulty = "[DIFFICULTY]";
    public int Reward = 300;
    public string Weaknesses = "[WEAKNESSES]";
    public int SealSlots = 6;

    public static Dictionary<YokaiId, YokaiData> YokaiList = (new List<YokaiData> {
        null,
        new YokaiData(){
            Id = YokaiId.Hitotsumekozo,
            Name = "Hitotsumekozo",
            Difficulty  = "EASY",
            Reward = 100,
            SealSlots = 6,
        },
        new YokaiData(){
            Id = YokaiId.Kasaobake,
            Name = "Kasa-Obake",
            Difficulty  = "MEDIUM",
            Reward = 250,
            SealSlots = 8,
        },
        new YokaiData(){
            Id = YokaiId.Chochinobake,
            Name = "Chochi-No-Bake",
            Difficulty  = "HARD",
            Reward = 500,
            SealSlots = 10,

        },
        new YokaiData(){
            Id = YokaiId.Jorogumo,
            Name = "Joro-Gumo",
            Difficulty  = "LEGENDARY",
            Reward = 1000,
            SealSlots = 12
        },
     }).ToDictionary((yokai) => yokai?.Id ?? YokaiId.None);
}