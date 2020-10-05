using System;
using Godot;

public class Food {
    public string Name;
    public int Index;
    public string Description;
    public int Price;
    public Action Effect;

    public static readonly Food ONIGIRI = new Food {
        Name = "Onigiri",
        Index = 0,
        Description = "Delicious Onigiri that increases max health by 1",
        Effect = () => GameData.Instance.MaxHealth += 1,
        Price = 200,
    };
    public static readonly Food SUSHI = new Food {
        Name = "Sushi",
        Index = 1,
        Description = "A single sushi for +1 max ki",
        Effect = () => GameData.Instance.MaxKi += 1,
        Price = 400,
    };
    public static readonly Food DANGO = new Food {
        Name = "Dango",
        Index = 2,
        Description = "Three dangos for +30% reward",
        Effect = () => GameData.Instance.MoneyPercentageBonus += 30,
        Price = 300,
    };
    public static readonly Food JAGAIMO = new Food {
        Name = "Jagaimo",
        Index = 3,
        Description = "Hot potato for +1 card drawn per turn",
        Effect = () => GameData.Instance.CardsPerTurn += 1,
        Price = 350,
    };
}