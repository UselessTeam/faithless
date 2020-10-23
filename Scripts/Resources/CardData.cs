using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

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
                Description = "Click on a slot to place a [fire-seal]\nAt the start of your turn, a [fire-seal] burns any surrounding [wood-seal], turning them into [fire-seal]s, and producing 1 Ki",
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
                Description = "Click on a slot to place a [wood-seal]\nIf a [wood-seal] is next to at least one [water-seal], harvest (draw 1 card)\nGain 1 Seed",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal (Element.Wood, useLocation); BattleScene.Seeds+=1; }
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
            new CardData {Id = CardId.BasicMetal, // Metal becomes en metal
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                Description = "Click on a slot to place a [metal-seal]\nIf an enemy attacks this seal, he is staggered for the next turn",
                Use = async (useLocation) => { await BattleScene.Instance.AddSeal(Element.Metal, useLocation); }
            },


            new CardData {Id = CardId.Flood,
                Name = "Flood",
                Kanji = "洪",
                Element = Element.Water,
                Cost = 0,
                Description = "Replace a group of continuous [earth-seal]s by [water-seal]s",
                Use = async (useLocation) => {
                    //TODO
                }
            },
            new CardData {Id = CardId.WashAway,
                Name = "Wash Away",
                Kanji = "",
                Element = Element.Water,
                Cost = 1,
                Target = CardTarget.Yokai,
                Description = "Discard all your cards\nDraw as many cards +1",
                Use = async (useLocation) => {
                    int cardCount =  BattleScene.Hand.Cards.Count();
                    await BattleScene.Hand.DiscardAll();
                    await BattleScene.DrawCards(cardCount);
                }
            },
            new CardData {Id = CardId.Tsunami,
                Name = "Tsunami",
                Kanji = "波",
                Element = Element.Water,
                Cost = 2,
                Target = CardTarget.EmptySeal,
                Description = "Target an empty area\nDiscard all your cards\nPlace one [water-seal] in the area for each card you discarded\nBanish this card",
                Use = async (useLocation) => {
                //TODO
                }
            },
            new CardData {Id = CardId.Watermill,
                Name = "Watermill",
                Kanji = "津",
                Element = Element.Water,
                Cost = 0,
                Description = "Replace a [water-seal] it by a random other element\nIf is replaced by an [earth-seal], gain 1 Ki",
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
                    if (BattleScene.SealSlots[useLocation] == Element.Earth) BattleScene.Ki +=1;

                }
            },
            new CardData {Id = CardId.HotSpring, // Too much use with fire
                Name = "Hot Spring",
                Kanji = "泉",
                Element = Element.Water,
                Cost = 2,
                Description = "Place a [water-seal]\nGain one health of each [fire-seal] adjacent to it",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Water, useLocation);
                    if (BattleScene.SealSlots[(useLocation+1)%sealCount] == Element.Fire)           BattleScene.Health += 1;
                    if (BattleScene.SealSlots[(useLocation+sealCount-1)%sealCount] == Element.Fire) BattleScene.Health += 1;
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
                Cost = 2,
                Target = CardTarget.MetalSeal,
                Description = "Place [metal-seal] on the selected location and the two adjacent locations",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Metal, useLocation);
                    await BattleScene.Instance.AddSeal(Element.Metal, ((useLocation+1)%sealCount));
                    await BattleScene.Instance.AddSeal(Element.Metal, ((useLocation+sealCount-1)%sealCount));
                }
            },
            new CardData {Id = CardId.Rust,
                Name = "Rust",
                Kanji = "",
                Element = Element.Metal,
                Cost = 0,
                Target = CardTarget.MetalSeal,
                Description = "Surrond a [metal-seal] by 2 [water-seal]",
                Use = async (useLocation) => {
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.AddSeal(Element.Water, (useLocation+1)%sealCount);
                    await BattleScene.Instance.AddSeal(Element.Water, (useLocation+sealCount-1)%sealCount);
                }
            },


            new CardData {Id = CardId.PineCone,
                Name = "PineCone",
                Kanji = "",
                Element = Element.Wood,
                Cost = 0,
                Description = "Gain 2 seeds",
                Use = async (useLocation) => {
                    BattleScene.Seeds+=2;
                }
            },
            new CardData {Id = CardId.Abundance,
                Name = "Abundance",
                Kanji = "穫",
                Element = Element.Wood,
                Cost = 1,
                Target = CardTarget.Yokai,
                Description = "Discard all cards in your hand\nGain 1 seed per card",
                Use = async (useLocation) => {
                    int cardCount =  BattleScene.Hand.Cards.Count() - 1;
                    await BattleScene.Hand.DiscardAll();
                    BattleScene.Seeds+=cardCount;
                }
            },
            new CardData {Id = CardId.Roots,
                Name = "Roots",
                Kanji = "根",
                Element = Element.Wood,
                Cost = 0,
                Description = "Draw the last card your discarded (if the discard is empty, draw one card instead)\nIf you drew a Water or Earth card, gain one seed",
                Use = async (useLocation) => {
                    if(!await BattleScene.Hand.DrawLastDiscard()) {
                        await BattleScene.Hand.DrawCard();
                    }
                    var lastDrawn = BattleScene.Hand.Cards.ToList()[BattleScene.Hand.Cards.Count() -1].Data();
                    if (lastDrawn.Element == Element.Water || lastDrawn.Element == Element.Earth){
                        BattleScene.Seeds+=1;
                    }
                }
            },
            new CardData {Id = CardId.Plantation,
                Name = "Plantation",
                Kanji = "農",
                Element = Element.Wood,
                Cost = 1,
                Target = CardTarget.EarthSeal,
                Description = "Replace a group of continuous [earth-seal]s by [wood-seal]s\nGain one seed per Seal replaced",
                Use = async (useLocation) => {
                    // TODO
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
                Cost = 1,
                Target = CardTarget.Yokai,
                Description = "Replace all [earth-seal] by [metal-seal]",
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Earth) { await BattleScene.Instance.SwitchSeal(Element.Metal, i); }
                    }
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
                    await BattleScene.Hand.DiscardAll();
                    await BattleScene.DrawCards(GameData.Instance.CardsPerTurn);
                    BattleScene.Health = GameData.Instance.MaxHealth;
                }
            },
            new CardData {Id = CardId.Cooking,
                Name = "Cooking",
                Kanji = "",
                Element = Element.Fire,
                Cost = 0,
                Target = CardTarget.Yokai,
                Description = "Remove all seeds\n Gain 1 Ki for each seed burned",
                Use = async (useLocation) => {
                    BattleScene.Ki += (short) BattleScene.Seeds;
                    BattleScene.Seeds = 0;
                }
            },


            new CardData {Id = CardId.Drought,
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Earth,
                Cost = 1,
                Description = "Discard all your Water and Wood talismans\n Replace all [water-seal] and [wood-seal] by [earth-seal]\nGain 1 Chi for each [water-seal] replaced\nDraw 1 card for each [wood-seal] replaced",
                Use = async (useLocation) => {
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
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
                    for (int i = 0 ; i < BattleScene.SealCount - 1; i++) {
                        tasks.Add(BattleScene.Instance.SealCircleField.MoveSeal(i+1, i, BattleScene.SealSlots[i+1]));
                        BattleScene.SealSlots[i]= BattleScene.SealSlots[i+1];
                    }
                    tasks.Add( BattleScene.Instance.SealCircleField.MoveSeal(0, BattleScene.SealSlots.Count -1, lastElement));
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
                Target = new CardTarget{ TargetDescription = "An empty Seal location adjacent to an [earth-seal]",
                    CheckTargetableFunc = (int loc) => { return BattleScene.SealSlots[(loc+1)%BattleScene.SealCount] == Element.Earth ||
                    BattleScene.SealSlots[(loc+BattleScene.SealCount-1)%BattleScene.SealCount] == Element.Earth; }
                },
                Description = "Place an [earth-seal] adjacent to another [earth-seal]",
                Use = async (useLocation) => {
                    await BattleScene.Instance.AddSeal(Element.Earth, useLocation);
                }
            },
            new CardData {Id = CardId.Tectonic,
                Name = "Tectonic",
                Kanji = "地",
                Element = Element.Earth,
                Cost = 1,
                Description = "Remove a Seal and turn it into a card that costs 0 Ki\n",
                Use = async (useLocation) => {
                    //await BattleScene.Instance.RemoveSeal(useLocation);
                    //TODO
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

