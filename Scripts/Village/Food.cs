using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Utils;

public enum FoodId {
    None,
    Onigiri,
    Sushi,
    Dango,
    Jagaimo
}

public class FoodData {
    public FoodId Id;
    public string Name;
    public string Description;
    public int Price;
    public Action Effect;

    private static Dictionary<FoodId, FoodData> list = (new FoodData[] {
    null,
    new FoodData {
        Name = "Onigiri",
        Id = FoodId.Onigiri,
        Description = "Delicious Onigiri that increases max health by 1",
        Effect = () => GameData.Instance.MaxHealth += 1,
        Price = 200,
    },
    new FoodData {
        Name = "Sushi",
        Id = FoodId.Sushi,
        Description = "A single sushi for +1 max ki",
        Effect = () => GameData.Instance.MaxKi += 1,
        Price = 500,
    },
    new FoodData {
        Name = "Dango",
        Id = FoodId.Dango,
        Description = "Three dangos for +30% reward",
        Effect = () => GameData.Instance.MoneyPercentageBonus += 30,
        Price = 300,
    },
    new FoodData {
        Name = "Jagaimo",
        Id = FoodId.Jagaimo,
        Description = "Hot potato for +1 card drawn per turn",
        Effect = () => GameData.Instance.CardsPerTurn += 1,
        Price = 350,
    }
    }).ToDictionary((FoodData food) => food?.Id ?? FoodId.None);

    public static FoodData Find (FoodId id) => list[id];
}