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
    // Flood,
    Watermill,
    HotSpring,
    Tsunami,
    Recycle,
    Forge,
    SteelTools,
    Stronghold,
    Abundance,
    Roots,
    Plantation,
    RiceField,
    Eruption,
    Combustion,
    Phoenix,
    FireSpread,
    Drought,
    Landslide,
    Carving,
    Tectonic,
    TOTAL, // Leave at the end
}

public static class CardIdExtensions {
    public static CardData Data (this CardId id) {
        return CardData.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }
public class CardData : Resource {
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

    public bool BanishAfterUse = false;

    /*** Methods ***/
    private CardData () { }

    private static Dictionary<CardId, CardData> list = (new CardData[] {
            null,
            new CardData {
                Id = CardId.BasicFire,
                Name = "Fire Seal",
                Kanji = "火",
                Element = Element.Fire,
                Cost = 2,
                Description = "Click on a slot to place a [fire-seal]\nAt the start of your turn, a [fire-seal] burns any surrounding [wood-seal] to produce 1 Ki",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Fire, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWater,
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                Description = "Click on a slot to place a [water-seal]\nPlace a [water-seal] on a [fire-seal] to estinguish it and recover 1 Health",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Water, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWood,
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                Description = "Click on a slot to place a [wood-seal]\nIf a [wood-seal] is next to at least one [water-seal], harvest and draw 1 card",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal (Element.Wood, useLocation); }
            },
            new CardData {
                Id = CardId.BasicEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                Description = "Click on a slot to place a [earth-seal]\nWhen you place a [earth-seal], pushes the Seal it was placed on clockwise",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Earth, useLocation); }
            },
            new CardData {Id = CardId.BasicMetal,
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                Description = "Click on a slot to place a [metal-seal]\nIf an enemy attacks this seal, he is staggered for the next turn",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Metal, useLocation); }
            },
            // new CardData {Id = CardId.Flood, //TODO
            //     Name = "Flood",
            //     Kanji = "洪",
            //     Element = Element.Water,
            //     Cost = 2,
            //     Description = "Place one [water-seal]\nReplace half of the seals on the sealing circle by [water-seal]",
            //     Use = async (useLocation) => {
            //         GD.Print("TODO Flood");
            //     }
            // },
            new CardData {Id = CardId.Tsunami,
                Name = "Tsunami",
                Kanji = "波",
                Element = Element.Water,
                Cost = 1,
                Description = "Discard all your cards\nDraw as many cards",
                Use = async (useLocation) => {
                    int cardCount =  BattleScene.Instance.Hand.Cards.Count();
                    await BattleScene.Instance.Hand.DiscardAll();
                    await BattleScene.DrawCards((byte)cardCount);
                }
            },
            new CardData {Id = CardId.Watermill,
                Name = "Watermill",
                Kanji = "津",
                Element = Element.Water,
                Cost = 0,
                Description = "Select a [water-seal] and replace it by a random other element",
                RequiredElement = Element.Water,
                Use = async (useLocation) => {
                    var elmIdx = Utils.RNG.rng.Next(0,4);
                    await BattleScene.Instance.SwitchSeal(elmIdx switch {
                            0 => Element.Fire,
                            1 => Element.Wood,
                            2 => Element.Earth,
                            _ => Element.Metal,
                        }, useLocation
                    );

                }
            },
            new CardData {Id = CardId.HotSpring,
                Name = "Hot Spring",
                Kanji = "泉",
                Element = Element.Water,
                Cost = 1,
                Description = "Gain one health of each [water-seal] placed next to a [fire-seal]",
                Use = async (useLocation) => {
                    byte sealCount = (byte) BattleScene.SealSlots.Count;
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water){
                            if (BattleScene.SealSlots[(useLocation+1)%sealCount] == Element.Fire
                                || BattleScene.SealSlots[(useLocation+sealCount-1)%sealCount] == Element.Fire)
                                BattleScene.Health += 1;
                        }
                    }
                }
            },
            new CardData {Id = CardId.Recycle,
                Name = "Recycle",
                Kanji = "再",
                Element = Element.Metal,
                Cost = 1,
                Description = "Remove one [metal-seal]. The next card you play will be free",
                RequiredElement = Element.Metal,
                Use = async (useLocation) => {
                    BattleScene.Instance.NextCardFree = true;
                    await BattleScene.Instance.RemoveSeal(useLocation);
                }
            },
            new CardData {Id = CardId.Forge,
                Name = "Forge",
                Kanji = "鍛",
                Element = Element.Metal,
                Cost = 3,
                Description = "Place one [metal-seal] on the selected location, and another one on the opposite side of the circle",
                Use = async (useLocation) => {
                    await BattleScene.Instance.AddSeal(Element.Metal, useLocation);
                    await BattleScene.Instance.AddSeal(Element.Metal, (byte)((useLocation + BattleScene.SealSlots.Count/2) % BattleScene.SealSlots.Count));
                }
            },
            new CardData {Id = CardId.SteelTools,
                Name = "Steel Tools",
                Kanji = "鋼",
                Element = Element.Metal,
                Cost = 3,
                BanishAfterUse = true,
                Description = "Harvesting from [wood-seal] grants one more hand\nBanish this card until the end of the combat",
                Use = async (useLocation) => {
                    BattleScene.Instance.HarvestBonus +=1;
                }
            },
            new CardData {Id = CardId.Stronghold,
                Name = "Stronghold",
                Kanji = "砦",
                Element = Element.Metal,
                Cost = 4,
                Description = "Place [metal-seal] on the selected location and the two adjacent locations",
                Use = async (useLocation) => {
                    byte sealCount = (byte) BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Metal, useLocation);
                    await BattleScene.Instance.AddSeal(Element.Metal, (byte)((useLocation+1)%sealCount));
                    await BattleScene.Instance.AddSeal(Element.Metal, (byte)((useLocation+sealCount-1)%sealCount));
                }
            },
            new CardData {Id = CardId.Abundance,
                Name = "Abundance",
                Kanji = "穫",
                Element = Element.Wood,
                Cost = 2,
                Description = "Harvest one card for each [wood-seal] on the sealing circle",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Wood) await BattleScene.DrawCards((byte)(1+BattleScene.Instance.HarvestBonus));
                    }
                }
            },
            new CardData {Id = CardId.Roots,
                Name = "Roots",
                Kanji = "根",
                Element = Element.Wood,
                Cost = 0,
                Description = "Draw the last card your discarded\nIf the discard is empty, draw one card",
                Use = async (useLocation) => {
                    if(BattleScene.Discard.Count >0) {
                        BattleScene.Deck.Insert(0, BattleScene.Discard[BattleScene.Discard.Count-1]);
                        BattleScene.Discard.RemoveAt(BattleScene.Discard.Count-1);
                    }
                    await BattleScene.DrawCards(1);
                }
            },
            new CardData {Id = CardId.Plantation,
                Name = "Plantation",
                Kanji = "農",
                Element = Element.Wood,
                Cost = 0,
                Description = "Select one [earth-seal] and replace it by a [wood-seal]",
                RequiredElement = Element.Earth,
                Use = async (useLocation) => {
                    await BattleScene.Instance.SwitchSeal(Element.Wood, useLocation);
                }
            },
            new CardData {Id = CardId.RiceField,
                Name = "Rice Field",
                Kanji = "田",
                Element = Element.Wood,
                Cost = 1,
                Description = "Select one [water-seal], and place one [wood-seal] in a random empty space next to it",
                RequiredElement = Element.Water,
                Use = async (useLocation) => {
                    byte sealCount = (byte) BattleScene.SealSlots.Count;
                    byte sealBefore = (byte)((useLocation+sealCount-1)%sealCount);
                    byte sealAfter = (byte)((useLocation+1)%sealCount);
                    int noneCount =0;
                    if(BattleScene.SealSlots[sealAfter] == Element.None) noneCount ++;
                    if(BattleScene.SealSlots[sealBefore] == Element.None) noneCount +=2;
                    if(noneCount == 0) return;
                    else if(noneCount == 1) await BattleScene.Instance.AddSeal(Element.Wood, sealAfter);
                    else if(noneCount == 2) await BattleScene.Instance.AddSeal(Element.Wood, sealBefore);
                    else{
                        if(Utils.RNG.rng.Next(0,2) == 0)
                            await BattleScene.Instance.AddSeal(Element.Wood, sealAfter);
                        else
                            await BattleScene.Instance.AddSeal(Element.Wood, sealBefore);
                    }
                }
            },
            new CardData {Id = CardId.Eruption,
                Name = "Eruption",
                Kanji = "噴",
                Element = Element.Fire,
                Cost = 3,
                Description = "Replace all [earth-seal] by [metal-seal]\nGain one Chi by Seal replaced\nDraw 1 Cards",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Earth) { await BattleScene.Instance.SwitchSeal(Element.Metal, i); BattleScene.Ki+=1; }
                    }
                    await BattleScene.DrawCards(1);
                }
            },
            new CardData {Id = CardId.Combustion,
                Name = "Combustion",
                Kanji = "燃",
                Element = Element.Fire,
                Cost = 0,
                RequireFull = true,
                Description = "Destroy a Seal\nDraw 3 Cards",
                Use = async (useLocation) => {
                    await BattleScene.Instance.RemoveSeal(useLocation);
                    await BattleScene.DrawCards(3);
                }
            },
            new CardData {Id = CardId.FireSpread,
                Name = "Fire Spread",
                Kanji = "災",
                Element = Element.Fire,
                Cost = 2,
                RequiredElement = Element.Fire,
                Description = "Select one [fire-seal]\nPlace a [fire-seal] on surrounding locations",
                Use = async (useLocation) => {
                    byte sealCount = (byte) BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Fire, (byte)((useLocation+1)%sealCount));
                    await BattleScene.Instance.AddSeal(Element.Fire, (byte)((useLocation+sealCount-1)%sealCount));
                }
            },
            new CardData {Id = CardId.Phoenix,
                Name = "Phoenix",
                Kanji = "鵬",
                Element = Element.Fire,
                Cost = 4,
                Description = "Destroy all Seals\nGain 2 Chi for each Seal destroyed\nDraw 4 Cards\nHeal to full health",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if(BattleScene.SealSlots[i] != Element.None){
                       await BattleScene.Instance.RemoveSeal(i); BattleScene.Ki+=2;
                        }
                    }
                    await BattleScene.Instance.Hand.DiscardAll();
                    await BattleScene.DrawCards(4);
                    BattleScene.Health = GameData.Instance.MaxHealth;
}
            },
            new CardData {Id = CardId.Drought,
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Earth,
                Cost = 1,
                Description = "Replace all [water-seal] and [wood-seal] by [earth-seal]\nGain 1 Chi for each [water-seal] replaced\nDraw one card for each [wood-seal] replaced",
                Use = async (useLocation) => {
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water) { await BattleScene.Instance.SwitchSeal(Element.Earth, i); BattleScene.Ki += 1; }
                        if (BattleScene.SealSlots[i] == Element.Wood) { await BattleScene.Instance.SwitchSeal(Element.Earth, i);  await BattleScene.DrawCards(1);}
                    }
                }
            },
            new CardData {Id = CardId.Landslide,
                Name = "Landslide",
                Kanji = "崩",
                Element = Element.Earth,
                Cost = 0,
                Description = "Rotate the sealing circle counter-clockwise\n",
                Use = async (useLocation) => {
                var lastElement = BattleScene.SealSlots[0];
                    List<Task> tasks = new List<Task>();
                    for (byte i = 0 ; i < BattleScene.SealSlots.Count - 1; i++) {
                        tasks.Add(BattleScene.Instance.SealCircleField.MoveSeal((byte)(i+1),i,BattleScene.SealSlots[i+1]));
                        BattleScene.SealSlots[i]= BattleScene.SealSlots[i+1];
                    }
                    tasks.Add( BattleScene.Instance.SealCircleField.MoveSeal(0,(byte)(BattleScene.SealSlots.Count -1 ),lastElement));
                    foreach (Task task in tasks) {
                        await task;
                    }
                    BattleScene.SealSlots[BattleScene.SealSlots.Count -1] = lastElement;
                    BattleScene.Instance.SealCircleField.DisplaySeals();
                }
            },
            new CardData {Id = CardId.Carving,
                Name = "Carving",
                Kanji = "彫",
                Element = Element.Earth,
                Cost = 1,
                Description = "Select a location and place a copy of the Seal on the opposide side of the sealing circle",
                Use = async (useLocation) => {
                    await BattleScene.Instance.SwitchSeal(BattleScene.SealSlots[(useLocation + BattleScene.SealSlots.Count/2) % BattleScene.SealSlots.Count], useLocation);
                }
            },
            new CardData {Id = CardId.Tectonic,
                Name = "Tectonic",
                Kanji = "地",
                Element = Element.Earth,
                Cost = 1,
                Description = "Place an [earth-seal] and swap the surrounding Seals",
                Use = async (useLocation) => {
                    byte sealCount = (byte) BattleScene.SealSlots.Count;
                    byte sealBefore = (byte)((useLocation+sealCount-1)%sealCount);
                    byte sealAfter = (byte)((useLocation+1)%sealCount);

                    await BattleScene.Instance.SwitchSeal(Element.Earth, useLocation);
                    await BattleScene.Instance.SealCircleField.MoveSeal(sealBefore,sealAfter,BattleScene.SealSlots[sealBefore]);
                    await BattleScene.Instance.SealCircleField.MoveSeal(sealAfter,sealBefore,BattleScene.SealSlots[sealAfter]);

                    var swapElm = BattleScene.SealSlots[sealBefore];
                    BattleScene.SealSlots[sealBefore] = BattleScene.SealSlots[sealAfter];
                    BattleScene.SealSlots[sealAfter] = swapElm;

                    BattleScene.Instance.SealCircleField.DisplaySeals();
                }

            },

        }).ToDictionary((card) => card?.Id ?? CardId.None);

    public static CardData Find (CardId id) {
        try {
            return list[id];
        } catch {
            GD.PrintErr($"Trying to access a non-existing card {id}");
            return null;
        }
    }
    public static bool CheckPlayable (CardId id, Element currentElement) {
        CardData card = Find(id);
        if (card.RequiredElement != Element.None && card.RequiredElement != currentElement)
            return false;
        if (card.RequireEmpty && currentElement != Element.None)
            return false;
        if (card.RequireFull && currentElement == Element.None)
            return false;
        return true;
    }
}

