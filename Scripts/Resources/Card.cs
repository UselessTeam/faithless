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

public static class CardIdExtensions {
    public static Card CardType (this CardId id) {
        return Card.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }
public class Card : Resource {
    // [Export] readonly ushort Id = ushort.MaxValue;
    [Export] public string Name { get; init; } = "Unamed";
    [Export] public string Kanji { get; init; } = "無";
    [Export] public Element Element { get; init; } = Element.None;
    [Export] public byte Cost { get; init; } = byte.MaxValue;
    [Export] public string Description { get; init; } = "This is an empty card";
    [Export] public Action<byte> Use { get; init; } = (byte id) => { GD.Print("This card does nothing"); };
    public Texture Texture => CardTextures.Instance.GetTexture(Element);

    private Card () { }
    private Card (Element element, byte cost, string description, Action<byte> use) { this.Element = element; this.Cost = cost; this.Description = description; this.Use = use; }

    private static Dictionary<CardId, Card> list =
        new Dictionary<CardId, Card> {
            {CardId.BasicFire,
            new Card {
                Name = "Fire Seal",
                Kanji = "火",
                Element = Element.Fire,
                Cost = 2,
                Description = "Place one Fire Seal",
                Use = (useLocation) => { BattleScene.Instance.AddSeal(Element.Fire, useLocation); }
            }},
            {CardId.BasicWater,
            new Card {
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                Description = "Place one Water Seal",
                Use = (useLocation) => { BattleScene.Instance.AddSeal(Element.Water, useLocation); }
            }},
            {CardId.BasicWood,
            new Card {
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                Description = "Place one Wood Seal",
                Use = (useLocation) => { BattleScene.Instance.AddSeal(Element.Wood, useLocation); }
            }},
            {CardId.BasicEarth,
            new Card {
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                Description = "Place one Earth Seal",
                Use = (useLocation) => { BattleScene.Instance.AddSeal(Element.Earth, useLocation); }
            }},
            {CardId.BasicMetal,
            new Card {
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                Description = "Place one Metal Seal",
                Use = (useLocation) => { BattleScene.Instance.AddSeal(Element.Metal, useLocation); }
            }},
        };

    public static Card Find (CardId id) {
        return list[id];
    }
}

