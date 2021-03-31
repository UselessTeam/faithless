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
    public int DifficultyValue = 2;
    public int Reward = 300;
    public string Weaknesses = "[WEAKNESSES]";
    public int SealSlots = 6;

    public static Dictionary<YokaiId, YokaiData> YokaiList = (new List<YokaiData> {
        null,
        new YokaiData(){
            Id = YokaiId.Hitotsumekozo,
            Name = "Hitotsumekozo",
            Difficulty  = "EASY",
            DifficultyValue = 2,
            Reward = 100,
            SealSlots = 6,
        },
        new YokaiData(){
            Id = YokaiId.Kasaobake,
            Name = "Kasa-Obake",
            Difficulty  = "MEDIUM",
            DifficultyValue = 3,
            Reward = 200,
            SealSlots = 8,
        },
        new YokaiData(){
            Id = YokaiId.Chochinobake,
            Name = "Chochi-No-Bake",
            Difficulty  = "HARD",
            DifficultyValue = 4,
            Reward = 300,
            SealSlots = 10,

        },
        new YokaiData(){
            Id = YokaiId.Jorogumo,
            Name = "Joro-Gumo",
            Difficulty  = "LEGENDARY",
            DifficultyValue = 5,
            Reward = 600,
            SealSlots = 12
        },
     }).ToDictionary((yokai) => yokai?.Id ?? YokaiId.None);
}