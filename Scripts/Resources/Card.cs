using System;
using System.Collections.Generic;
using Godot;

public enum CardId {
    BasicFire,
    BasicWater,
    BasicWood,
    BasicEarth,
    BasicMetal
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }

public class Card : Resource {
    // [Export] readonly ushort Id = ushort.MaxValue;
    [Export] readonly byte Cost = byte.MaxValue;
    [Export] readonly string Description = "This is an empty card";
    [Export] readonly Element Element = Element.None;
    [Export] readonly Action<byte> Use = (byte id) => { GD.Print("This card does nothing"); };

    public Card () { }
    public Card (Element element, byte cost, string description, Action<byte> use) { this.Element = element; this.Cost = cost; this.Description = description; this.Use = use; }

    public static Dictionary<CardId, Card> List =
        new Dictionary<CardId, Card> {
            {
            CardId.BasicFire,
            new Card (Element.Fire, 2, "Place one Fire Seal",
            (useLocation) => { BattleScene.Instance.AddSeal(Element.Fire, useLocation); })
            },
            {
            CardId.BasicWater,
            new Card (Element.Water,2, "Place one Water Seal",
            (useLocation) => { BattleScene.Instance.AddSeal(Element.Water, useLocation); })
            },
            {
            CardId.BasicWood,
            new Card (Element.Wood, 2, "Place one Wood Seal",
            (useLocation) => { BattleScene.Instance.AddSeal(Element.Wood, useLocation); })
            },
            {
            CardId.BasicEarth,
            new Card (Element.Earth, 2, "Place one Earth Seal",
            (useLocation) => { BattleScene.Instance.AddSeal(Element.Earth, useLocation); })
            },
            {
            CardId.BasicMetal,
            new Card (Element.Metal, 2, "Place one Metal Seal",
            (useLocation) => { BattleScene.Instance.AddSeal(Element.Metal, useLocation); })
            },
        };
}

