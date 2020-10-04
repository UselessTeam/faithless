using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;


public enum CardId {
    None,
    BasicFire,
    BasicWater,
    BasicWood,
    BasicEarth,
    BasicMetal,
    Drought,
    Harvest,
    Flood,
    Forge,
    TOTAL, // Leave at the end
}

public static class CardIdExtensions {
    public static Card Data (this CardId id) {
        return Card.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }
public class Card : Resource {
    // [Export] readonly ushort Id = ushort.MaxValue;
    private static List<CardId> GenerateAll () {
        List<CardId> all = new List<CardId>();
        for (int i = 1 ; i < (int) CardId.TOTAL ; i++) {
            all.Add((CardId) i);
        }
        return all;
    }
    private static List<CardId> GenerateAllSpecial () {
        List<CardId> all = new List<CardId>();
        for (int i = 6 ; i < (int) CardId.TOTAL ; i++) {
            all.Add((CardId) i);
        }
        return all;
    }
    public static List<CardId> All { get; } = GenerateAll();
    public static List<CardId> AllSpecial { get; } = GenerateAllSpecial();

    /*** Fields ***/
    public CardId Id = CardId.None;
    public string Name = "Unamed";
    public string Kanji = "無";
    public Element Element = Element.None;
    public byte Cost = byte.MaxValue;
    public int MonPrice = 200;
    public string Description = "This is an empty card";
    public Func<byte, Task> Use = (byte id) => { GD.PrintErr("This card does nothing"); return null; };
    public Texture Texture => CardTextures.Instance.GetTexture(Element);

    // Requirement
    public Element RequiredElement = Element.None;
    public bool RequireEmpty = false;
    public bool RequireFull = false;

    /*** Methods ***/
    private Card () { }

    private static Dictionary<CardId, Card> list = (new Card[] {
            null,
            new Card {
                Id = CardId.BasicFire,
                Name = "Fire Seal",
                Kanji = "火",
                Element = Element.Fire,
                Cost = 2,
                Description = "Place one Fire Seal",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Fire, useLocation); }
            },
            new Card {
                Id = CardId.BasicWater,
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                Description = "Place one Water Seal",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Water, useLocation); }
            },
            new Card {
                Id = CardId.BasicWood,
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                Description = "Place one Wood Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal (Element.Wood, useLocation); }
            },
            new Card {
                Id = CardId.BasicEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                Description = "Place one Earth Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Earth, useLocation); }
            },
            new Card {
                Id = CardId.BasicMetal,
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                Description = "Place one Metal Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Metal, useLocation); }
            },
            new Card {
                Id = CardId.Drought,
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Fire,
                Cost = 1,
                Description = "Remove all Water Seals and gain 2 Chi for each one discarded\nReplace all Wood Seals by Earth Seals",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water) { await BattleScene.Instance.RemoveSeal(i); BattleScene.Chi += 2; }
                        if (BattleScene.SealSlots[i] == Element.Wood) { await BattleScene.Instance.SwitchSeal(Element.Earth, i); }
                    }
                }
            },
            new Card {
                Id = CardId.Harvest,
                Name = "Harvest",
                Kanji = "穫",
                Element = Element.Wood,
                Cost = 2,
                Description = "Draw one card for each Wood Seal on the sealing circle",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Wood) await BattleScene.DrawCards(1);
                    }
                }
            },

            new Card {
                Id = CardId.Forge,
                Name = "Forge",
                Kanji = "鍛",
                Element = Element.Wood,
                Cost = 1,
                Description = "Remove one Metal Seal. The next card you play will be free",
                Use = async (useLocation) => {
                    if (BattleScene.SealSlots[useLocation] == Element.Metal) {
                        await BattleScene.Instance.RemoveSeal(useLocation);
                        GD.Print("TODO: Next card is Free");
                    } else
                        GD.Print("TODO: Abort using a card");
                }
            },
            new Card {
                Id = CardId.Flood,
                Name = "Flood",
                Kanji = "洪",
                Element = Element.Water,
                Cost = 1,
                Description = "Replace half of the seals on the sealing circle by Water Seals",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        // if (BattleScene.SealSlots[i] != Element.Wood) await BattleScene.DrawCards(1);
                    }
                }
            }
        }).ToDictionary((card) => card?.Id ?? CardId.None);
    // Landslide,
    // Eruption,
    // Confusion,
    public static Card Find (CardId id) {
        return list[id];
    }
    public static bool CheckPlayable (CardId id, Element currentElement) {
        Card card = Find(id);
        if (card.RequiredElement != Element.None && card.RequiredElement != currentElement)
            return false;
        if (card.RequireEmpty && currentElement != Element.None)
            return false;
        if (card.RequireFull && currentElement == Element.None)
            return false;
        return true;
    }
}

