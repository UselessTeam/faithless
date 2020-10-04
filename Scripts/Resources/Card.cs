using System;
using System.Collections.Generic;
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
}

public static class CardIdExtensions {
    public static Card Data (this CardId id) {
        return Card.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }
public class Card : Resource {
    // [Export] readonly ushort Id = ushort.MaxValue;
    public string Name = "UnamCardId.BasicEarth, CardId.BasicFire, CardId.BasicMetal, CardId.BasicWater, CardId.BasicWooded";
    public string Kanji = "無";
    public Element Element = Element.None;
    public byte Cost = byte.MaxValue;
    public string Description = "This is an empty card";
    public Func<byte, Task> Use = (byte id) => { GD.PrintErr("This card does nothing"); return null; };
    public Texture Texture => CardTextures.Instance.GetTexture(Element);

    private Card () { }
    // private Card (Element element, byte cost, string description, Action<byte> use) { this.Element = element; this.Cost = cost; this.Description = description; this.Use = use; }


    private static Dictionary<CardId, Card> list =
        new Dictionary<CardId, Card> {
            {CardId.None, null},
            {CardId.BasicFire,new Card {
                Name = "Fire Seal",
                Kanji = "火",
                Element = Element.Fire,
                Cost = 2,
                Description = "Place one Fire Seal",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Fire, useLocation); }
            }},
            {CardId.BasicWater, new Card {
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                Description = "Place one Water Seal",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Water, useLocation); }
            }},
            {CardId.BasicWood, new Card {
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                Description = "Place one Wood Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Wood, useLocation);  }
            }},
            {CardId.BasicEarth,
                new Card {
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                Description = "Place one Earth Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Earth, useLocation); }
            }},
            {CardId.BasicMetal,
                new Card {
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                Description = "Place one Metal Seal",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Metal, useLocation); }
            }},
            {CardId.Drought,
                new Card {
                Name = "Drought",
                Kanji = "金",
                Element = Element.Fire,
                Cost = 1,
                Description = "Remove all Water Seals and gain 2 Chi for each one discarded\nReplace all Wood Seals by Earth Seals",
                Use = async (useLocation) => {
                    for(byte i =0; i< BattleScene.SealSlots.Count; i ++){
                        if (BattleScene.SealSlots[i] == Element.Water){ await  BattleScene.Instance.RemoveSeal(i); BattleScene.Chi+=2; }
                        if (BattleScene.SealSlots[i] == Element.Wood){ await BattleScene.Instance.SwitchSeal(Element.Earth, i); }
                    }
                }
            }},
            {CardId.Harvest,
                new Card {
                Name = "Harvest",
                Kanji = "金",
                Element = Element.Wood,
                Cost = 2,
                Description = "Draw one card for each Wood Seal on the sealing circle",
                Use = async (useLocation) => {
                    for(byte i =0; i< BattleScene.SealSlots.Count; i ++){
                        if (BattleScene.SealSlots[i] == Element.Wood) await BattleScene.DrawCards(1);
                    }
                }
            }},
            {CardId.Forge,
                new Card {
                Name = "Forge",
                Kanji = "金",
                Element = Element.Wood,
                Cost = 1,
                Description = "Remove one Metal Seal. The next card you play will be free",
                Use = async (useLocation) => {
                        if (BattleScene.SealSlots[useLocation] == Element.Metal) {
                            await BattleScene.Instance.RemoveSeal(useLocation);
                            GD.Print("TODO: Next card is Free");
                        }
                        else
                            GD.Print("TODO: Abort using a card");
                    }
                }
            },
            {CardId.Flood,
                new Card {
                Name = "Flood",
                Kanji = "金",
                Element = Element.Water,
                Cost = 1,
                Description = "Replace half of the seals on the sealing circle by Water Seals",
                Use = async (useLocation) => {
                    for(byte i =0; i<BattleScene.SealSlots.Count; i ++){
                        // if (BattleScene.SealSlots[i] != Element.Wood) await BattleScene.DrawCards(1);
                    }

                }
            }}
        };
    // Landslide,
    // Eruption,
    // Confusion,
    public static Card Find (CardId id) {
        return list[id];
    }
}

