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
    // Add Free seal
}

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
    public int Cost = int.MaxValue;
    public int MonPrice = 200;
    public string Description = "This is an empty card";
    public Func<int, Task> Use = (int id) => { GD.PrintErr("This card does nothing"); return null; };
    public Texture Texture => CardTextures.Instance.GetTexture(Element);
    public CardTarget Target = CardTarget.AnySeal;

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
                Description = "Click on a slot to place a [fire-seal]\nAt the start of your turn, a [fire-seal] burns any surrounding [wood-seal] to produce 1 Ki", //TODO expliquer que ca remlace par du feu
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Fire, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWater,
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                Description = "Click on a slot to place a [water-seal]\nPlace a [water-seal] on a [fire-seal] to extinguish it and recover 1 Health",
                Use =  async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Water, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWood,
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                Description = "Click on a slot to place a [wood-seal]\nIf a [wood-seal] is next to at least one [water-seal], harvest (draw 1 card)",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal (Element.Wood, useLocation); }
            },
            new CardData { // Make it swap surroundings elements
                Id = CardId.BasicEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                Description = "Click on a slot to place a [earth-seal]\nWhen you place a [earth-seal], pushes the Seal it was placed on clockwise",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Earth, useLocation); }
            },
            new CardData {Id = CardId.BasicMetal, // Metal becomes en metal
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
                Target = CardTarget.Yokai,
                Description = "Discard all your cards\nDraw as many cards",
                Use = async (useLocation) => {
                    int cardCount =  BattleScene.Instance.Hand.Cards.Count();
                    await BattleScene.Instance.Hand.DiscardAll();
                    await BattleScene.DrawCards(cardCount);
                }
            },
            new CardData {Id = CardId.Watermill,
                Name = "Watermill",
                Kanji = "津",
                Element = Element.Water,
                Cost = 0,
                Description = "Select a [water-seal] and replace it by a random other element",
                Target = CardTarget.WaterSeal,
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
            new CardData {Id = CardId.HotSpring, // Too much use with fire
                Name = "Hot Spring",
                Kanji = "泉",
                Element = Element.Water,
                Cost = 1,
                Description = "Gain one health of each [water-seal] placed next to a [fire-seal]",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
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
                Target = CardTarget.MetalSeal,
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
                    await BattleScene.Instance.AddSeal(Element.Metal, ((useLocation + BattleScene.SealSlots.Count/2) % BattleScene.SealSlots.Count));
                }
            },
            new CardData {Id = CardId.SteelTools,
                Name = "Steel Tools",
                Kanji = "鋼",
                Element = Element.Metal,
                Cost = 3,
                BanishAfterUse = true,
                Description = "Harvesting from [wood-seal] grants one more hand\nBanish this card until the end of the combat", // TODO ne se banish pas
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
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Metal, useLocation);
                    await BattleScene.Instance.AddSeal(Element.Metal, ((useLocation+1)%sealCount));
                    await BattleScene.Instance.AddSeal(Element.Metal, ((useLocation+sealCount-1)%sealCount));
                }
            },
            // Metal does not become earth this turn
            new CardData {Id = CardId.Abundance,
                Name = "Abundance",
                Kanji = "穫",
                Element = Element.Wood,
                Cost = 2,
                Description = "Harvest one Ki for each [wood-seal] on the sealing circle", //Back to: draw 1 card
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Wood) BattleScene.Ki+=1;
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
                    if(!await BattleScene.Instance.Hand.DrawLastDiscard()) {
                        await BattleScene.Instance.Hand.DrawCard();
                    }
                }
            },
            new CardData {Id = CardId.Plantation,
                Name = "Plantation",
                Kanji = "農",
                Element = Element.Wood,
                Cost = 0,
                Description = "Select one [earth-seal] and replace a group of continuous [earth-seal] by [wood-seal]",
                Target = CardTarget.EarthSeal,
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
                Target = CardTarget.WaterSeal,
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    int sealBefore = ((useLocation+sealCount-1)%sealCount);
                    int sealAfter = ((useLocation+1)%sealCount);
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
                Description = "Replace all [earth-seal] by [metal-seal]\nGain one Chi by Seal replaced\nDraw 1 Cards", // Dont draw a card
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
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
                Target = CardTarget.NonEmptySeal,
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
                Target = CardTarget.FireSeal,
                Description = "Select one [fire-seal]\nPlace a [fire-seal] on surrounding locations",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Fire, ((useLocation+1)%sealCount));
                    await BattleScene.Instance.AddSeal(Element.Fire, ((useLocation+sealCount-1)%sealCount));
                }
            },
            new CardData {Id = CardId.Phoenix,
                Name = "Phoenix",
                Kanji = "鵬",
                Element = Element.Fire,
                Cost = 4,
                Description = "Destroy all Seals\nGain 2 Chi for each Seal destroyed\nDiscard all cards\nDraw new Cards (as a new turn)\nHeal to full health",
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if(BattleScene.SealSlots[i] != Element.None){
                            await BattleScene.Instance.RemoveSeal(i);
                            BattleScene.Ki += 2;
                        }
                    }
                    await BattleScene.Instance.Hand.DiscardAll();
                    await BattleScene.DrawCards(GameData.Instance.CardsPerTurn);
                    BattleScene.Health = GameData.Instance.MaxHealth;
}
            },
            new CardData {Id = CardId.Drought, //TOSO Update effect to description
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Earth,
                Cost = 1,
                Description = "Discard all your Water and Wood talismans. Replace all [water-seal] and [wood-seal] by [earth-seal]\nGain 2 Chi for each [water-seal] replaced\nDraw 2 card for each [wood-seal] replaced",
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water) { await BattleScene.Instance.SwitchSeal(Element.Earth, i); BattleScene.Ki += 2; }
                        if (BattleScene.SealSlots[i] == Element.Wood) { await BattleScene.Instance.SwitchSeal(Element.Earth, i);  await BattleScene.DrawCards(2);}
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
                    for (int i = 0 ; i < BattleScene.SealSlots.Count - 1; i++) {
                        tasks.Add(BattleScene.Instance.SealCircleField.MoveSeal((i+1),i,BattleScene.SealSlots[i+1]));
                        BattleScene.SealSlots[i]= BattleScene.SealSlots[i+1];
                    }
                    tasks.Add( BattleScene.Instance.SealCircleField.MoveSeal(0,(BattleScene.SealSlots.Count -1 ),lastElement));
                    foreach (Task task in tasks) {
                        await task;
                    }
                    BattleScene.SealSlots[BattleScene.SealSlots.Count -1] = lastElement;
                    BattleScene.Instance.SealCircleField.DisplaySeals();
                }
            },
            new CardData {Id = CardId.Carving, //TOO powerful
                Name = "Carving",
                Kanji = "彫",
                Element = Element.Earth,
                Cost = 1,
                Target = CardTarget.NonEmptySeal,
                Description = "Select a Seal and place a copy of it on the opposite side of the sealing circle",
                Use = async (useLocation) => {
                    await BattleScene.Instance.SwitchSeal(BattleScene.SealSlots[useLocation],( (useLocation + BattleScene.SealSlots.Count/2) % BattleScene.SealSlots.Count));
                }
            },
            new CardData {Id = CardId.Tectonic, //TODO REMOVE
                Name = "Tectonic",
                Kanji = "地",
                Element = Element.Earth,
                Cost = 1,
                Description = "Place an [earth-seal] and swap the surrounding Seals",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    int sealBefore = (useLocation+sealCount-1)%sealCount;
                    int sealAfter = (useLocation+1)%sealCount;

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
    public static bool CheckPlayable (CardId id, int location) {
        return id.Data().Target.CheckTargetableFunc(location);
    }
}

