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
    public string SFX = "GenericEffect";
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
                SFX = "TaikoLarge",
                Description = "Place a [fire-seal] on the selected slot \nAt the start of your turn, a [fire-seal] burns any surrounding [wood-seal], turning them into [fire-seals] and producing 1 [ki]",
                Use =  async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal(Element.Fire, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWater,
                Name = "Water Seal",
                Kanji = "水",
                Element = Element.Water,
                Cost = 2,
                SFX = "TaikoFunky",
                Description = "Place a [water-seal] on the selected slot \nIf an enemy tries to remove this Seal, it will turn into an [earth-seal] instead",
                Use =  async (useLocation) => {
                     await BattleScene.Instance.PlaceSeal(Element.Water, useLocation); }
            },
            new CardData {
                Id = CardId.BasicWood,
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 2,
                SFX = "TaikoMedium",
                Description = "Place a [wood-seal] on the selected slot and receive one [seed]\n[wood-seal] [?harvest]harvest[/?] (draws one card) at the beginning of your turn if next to a [water-seal]",
                Use = async (useLocation) => {

                    await BattleScene.Instance.PlaceSeal (Element.Wood, useLocation); await BattleScene.AddSeeds(1); }
            },
            new CardData {
                Id = CardId.BasicEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                SFX = "PlaceSeal",
                Description = "Place a [earth-seal] on the selected slot\nSwap the position of the two adjacent Seals",
                Use = async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal(Element.Earth, useLocation); }
            },
            new CardData {Id = CardId.BasicMetal, // Metal becomes en metal
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                SFX = "TaikoSmall",
                Description = "Place a [metal-seal] on the selected slot\nWhen a Yokai attacks this Seal, they become [?stagger]staggered[/?] for the next turn, and this Seal turns into an [earth-seal]",
                Use = async (useLocation) => {
                     await BattleScene.Instance.PlaceSeal(Element.Metal, useLocation); }
            },


            new CardData {Id = CardId.Flood,
                Name = "Flood",
                Kanji = "洪",
                Element = Element.Water,
                Cost = 0,
                Target = CardTarget.EarthSeal,
                SFX = "Yo",
                Description = "Replace a group of continuous [earth-seals] by [water-seals]",
                Use = async (useLocation) => {
                     var area = CardEffectHelper.GetArea(useLocation, Element.Earth);
                    List<Task> tasks = new List<Task>();

                    int i = area.Item1;
                    do{
                        tasks.Add(BattleScene.Instance.PlaceSeal(Element.Water, i));
                        i = CardEffectHelper.NextLocation(i);
                    }while ( i != area.Item2);

                    foreach(var task in tasks) await task;
                }
            },
            new CardData {Id = CardId.WashAway,
                Name = "Wash Away",
                Kanji = "流",
                Element = Element.Water,
                Cost = 1,
                Target = CardTarget.Yokai,
                SFX = "GenericEffect",
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
                SFX = "YoLong",
                BanishAfterUse = true,
                Target = CardTarget.EmptySeal,
                Description = "Target an empty area\nDiscard all your cards\nPlace one [water-seal] in the area for each card you discarded\nBanish this card until the end of the combat",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    var area = CardEffectHelper.GetArea(useLocation, Element.None);
                    var cardCount = BattleScene.Hand.Cards.Count() - 1;
                    tasks.Add(BattleScene.Hand.DiscardAll());

                    List<int> possibleLocations = new List<int>();
                    {   int i = area.Item1;
                        do{
                            possibleLocations.Add(i);
                            i = CardEffectHelper.NextLocation(i);
                        }while ( i != area.Item2);
                    }
                    possibleLocations = Utils.RNG.RandomOrder(possibleLocations).ToList();
                    for (int i =0; i< possibleLocations.Count && cardCount>0; i++){
                        tasks.Add(BattleScene.Instance.PlaceSeal(Element.Water, possibleLocations[i]) );
                        cardCount--;
                    }
                    foreach(var task in tasks) await task;
                }
            },
            new CardData {Id = CardId.Watermill,
                Name = "Watermill",
                Kanji = "津",
                Element = Element.Water,
                Cost = 0,
                Target = CardTarget.WaterSeal,
                SFX = "Wow",
                Description = "Replace a [water-seal] by a random other element\nIf it is replaced by an [earth-seal], gain 1 [ki]",
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
                SFX = "TaikoSmall",
                Description = "Place a [water-seal]\nGain one health of each [fire-seal] adjacent to it",
                Use = async (useLocation) => {
                     int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.PlaceSeal(Element.Water, useLocation);
                    if (BattleScene.SealSlots[(useLocation+1)%sealCount] == Element.Fire)           BattleScene.Health += 1;
                    if (BattleScene.SealSlots[(useLocation+sealCount-1)%sealCount] == Element.Fire) BattleScene.Health += 1;
                }
            },


            new CardData {Id = CardId.Recycle,
                Name = "Recycle",
                Kanji = "再",
                Element = Element.Metal,
                Cost = 0,
                Target = CardTarget.MetalSeal,
                SFX = "Yo",
                Description = "Remove one [metal-seal]. The next card you play will be free",
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
                SFX = "YoLong",
                Description = "Place one [metal-seal] on the selected slot, and another one on the opposite side of the circle",
                Use = async (useLocation) => {
                     Task task = BattleScene.Instance.PlaceSeal(Element.Metal, useLocation);
                    await BattleScene.Instance.PlaceSeal(Element.Metal, ((useLocation + BattleScene.SealSlots.Count/2) % BattleScene.SealSlots.Count));
                    await task;
                }
            },
            new CardData {Id = CardId.SteelTools,
                Name = "Steel Tools",
                Kanji = "鋼",
                Element = Element.Metal,
                Cost = 3,
                SFX = "SuspensePercussion",
                BanishAfterUse = true,
                Target = CardTarget.Yokai,
                Description = "The [?harvest]harvest[/?] effect of [wood-seals] grants one more card\nBanish this card until the end of the combat",
                Use = async (useLocation) => {
                     BattleScene.Instance.HarvestBonus +=1;
                }
            },
            new CardData {Id = CardId.Stronghold,
                Name = "Stronghold",
                Kanji = "砦",
                Element = Element.Metal,
                Cost = 2,
                SFX = "TaikoLarge",
                Target = CardTarget.MetalSeal,
                Description = "Place [metal-seals] on the 2 locations surrounding the selected [metal-seal]",
                Use = async (useLocation) => {
                     int sealCount =  BattleScene.SealSlots.Count;
                    Task task = BattleScene.Instance.PlaceSeal(Element.Metal, ((useLocation+1)%sealCount));
                    await BattleScene.Instance.PlaceSeal(Element.Metal, ((useLocation+sealCount-1)%sealCount));
                    await task;
                }
            },
            new CardData {Id = CardId.Rust,
                Name = "Rust",
                Kanji = "錆",
                Element = Element.Metal,
                Cost = 2,
                SFX = "TaikoSmall",
                Target = CardTarget.MetalSeal,
                Description = "Surround a [metal-seal] by 2 [water-seal]",
                Use = async (useLocation) => {
                     int sealCount =  BattleScene.SealSlots.Count;
                    Task task = BattleScene.Instance.PlaceSeal(Element.Water, (useLocation+1)%sealCount);
                    await BattleScene.Instance.PlaceSeal(Element.Water, (useLocation+sealCount-1)%sealCount);
                    await task;
                }
            },


            new CardData {Id = CardId.PineCone,
                Name = "PineCone",
                Kanji = "松",
                Element = Element.Wood,
                Cost = 0,
                SFX="Yo",
                Target = CardTarget.Yokai,
                Description = "Gain 2 [seeds]",
                Use = async (useLocation) => {
                     await BattleScene.AddSeeds(2);
                }
            },
            new CardData {Id = CardId.Abundance,
                Name = "Abundance",
                Kanji = "穫",
                Element = Element.Wood,
                Cost = 1,
                SFX="SuspensePercussion",
                Target = CardTarget.Yokai,
                Description = "Discard all cards in your hand\nGain 1 [seed] per card",
                Use = async (useLocation) => {
                     int cardCount =  BattleScene.Hand.Cards.Count() - 1;
                    await BattleScene.Hand.DiscardAll();
                    await BattleScene.AddSeeds(cardCount);
                }
            },
            new CardData {Id = CardId.Roots,
                Name = "Roots",
                Kanji = "根",
                Element = Element.Wood,
                Cost = 0,
                SFX = "Wow",
                Target = CardTarget.Yokai,
                Description = "Draw the last card you discarded (if the discard is empty, draw one card instead)\nIf you drew a Water or Earth card, gain one [seed]",
                Use = async (useLocation) => {
                     if(!await BattleScene.Hand.DrawLastDiscard()) {
                        await BattleScene.Hand.DrawCard();
                    }
                    var lastDrawn = BattleScene.Hand.Cards.ToList()[BattleScene.Hand.Cards.Count() -1].Data();
                    if (lastDrawn.Element == Element.Water || lastDrawn.Element == Element.Earth){
                        await BattleScene.AddSeeds(1);
                    }
                }
            },
            new CardData {Id = CardId.Plantation,
                Name = "Plantation",
                Kanji = "農",
                Element = Element.Wood,
                Cost = 1,
                SFX = "Yo",
                Target = CardTarget.EarthSeal,
                Description = "Replace a group of continuous [earth-seals] by [wood-seals]\nGain one [seed] per Seal replaced",
                Use = async (useLocation) => {
                     var area = CardEffectHelper.GetArea(useLocation, Element.Earth);
                    List<Task> tasks = new List<Task>();
                    int seedCount = 0;
                    int i = area.Item1;
                    do{
                        tasks.Add(BattleScene.Instance.PlaceSeal(Element.Wood, i));
                        seedCount ++;
                        i = CardEffectHelper.NextLocation(i);
                    }while ( i != area.Item2);

                    foreach(var task in tasks) await task;
                    await BattleScene.AddSeeds(seedCount);
                }
            },
            new CardData {Id = CardId.RiceField,
                Name = "Rice Field",
                Kanji = "田",
                Element = Element.Wood,
                Cost = 1,
                Target = CardTarget.WaterSeal,
                SFX = "TaikoMedium",
                Description = "Select one [water-seal], and place one [wood-seal] in a random empty slot adjacent to it\nGain one [seed]",
                Use = async (useLocation) => {
                     int sealCount =  BattleScene.SealSlots.Count;
                    int sealBefore = ((useLocation+sealCount-1)%sealCount);
                    int sealAfter = ((useLocation+1)%sealCount);
                    int noneCount =0;
                    if(BattleScene.SealSlots[sealAfter] == Element.None) noneCount ++;
                    if(BattleScene.SealSlots[sealBefore] == Element.None) noneCount +=2;
                    if(noneCount == 0) return;
                    else if(noneCount == 1) await BattleScene.Instance.PlaceSeal(Element.Wood, sealAfter);
                    else if(noneCount == 2) await BattleScene.Instance.PlaceSeal(Element.Wood, sealBefore);
                    else{
                        if(Utils.RNG.rng.Next(0,2) == 0)
                            await BattleScene.Instance.PlaceSeal(Element.Wood, sealAfter);
                        else
                            await BattleScene.Instance.PlaceSeal(Element.Wood, sealBefore);
                    }
                    await BattleScene.AddSeeds(1);
                }
            },


            new CardData {Id = CardId.Eruption,
                Name = "Eruption",
                Kanji = "噴",
                Element = Element.Fire,
                Cost = 1,
                Target = CardTarget.Yokai,
                Description = "Replace all [earth-seals] by [metal-seals]",
                SFX = "Yo",
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
                SFX = "Wow",
                Description = "Destroy any Seal\nDraw 3 Cards",
                Use = async (useLocation) => {
                     Task task = BattleScene.Instance.RemoveSeal(useLocation);
                    await BattleScene.DrawCards(3);
                    await task;
                }
            },
            new CardData {Id = CardId.FireSpread,
                Name = "Fire Spread",
                Kanji = "災",
                Element = Element.Fire,
                Cost = 2,
                Target = CardTarget.FireSeal,
                SFX="TaikoLarge",
                Description = "Select one [fire-seal]\nPlace a [fire-seal] on surrounding locations",
                Use = async (useLocation) => {
                     Task t = BattleScene.Instance.PlaceSeal(Element.Fire, CardEffectHelper.NextLocation(useLocation));
                    await BattleScene.Instance.PlaceSeal(Element.Fire, CardEffectHelper.PrevLocation(useLocation));
                    await t;
                }
            },
            new CardData {Id = CardId.Phoenix,
                Name = "Phoenix",
                Kanji = "鵬",
                Element = Element.Fire,
                Cost = 4,
                Target = CardTarget.Yokai,
                SFX = "SuspensePercussion",
                Description = "Destroy all Seals\nGain 2 [ki] for each Seal destroyed\nDiscard all cards\nDraw new Cards (as a new turn)\nHeal to full health",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if(BattleScene.SealSlots[i] != Element.None){
                            tasks.Add(BattleScene.Instance.RemoveSeal(i));
                            BattleScene.Ki += 2;
                        }
                    }
                    await BattleScene.Hand.DiscardAll();
                    foreach(var task in tasks) await task;
                    await BattleScene.DrawCards(GameData.Instance.CardsPerTurn);
                    BattleScene.Health = GameData.Instance.MaxHealth;
                }
            },
            new CardData {Id = CardId.Cooking,
                Name = "Cooking",
                Kanji = "焼",
                Element = Element.Fire,
                Cost = 0,
                SFX = "GenericEffect",
                Target = CardTarget.Yokai,
                Description = "Remove all [seeds]\n Gain 1 [ki] for each [seed] removed",
                Use = async (useLocation) => {
                     BattleScene.Ki += (short) BattleScene.Seeds;
                    await BattleScene.AddSeeds(-BattleScene.Seeds);
                }
            },


            new CardData {Id = CardId.Drought,
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Earth,
                Cost = 1,
                Target  = CardTarget.Yokai,
                SFX= "Yo",
                Description = "Replace all [water-seal] and [wood-seal] by [earth-seal]\nGain 1 [ki] for each [water-seal] replaced\nDraw 1 card for each [wood-seal] replaced",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water) { tasks.Add(BattleScene.Instance.SwitchSeal(Element.Earth, i)); BattleScene.Ki += 1; }
                        if (BattleScene.SealSlots[i] == Element.Wood) { tasks.Add(BattleScene.Instance.SwitchSeal(Element.Earth, i));  tasks.Add(BattleScene.DrawCards(1));}
                    }
                    foreach(var task in tasks) await task;
                }
            },
            new CardData {Id = CardId.Landslide,
                Name = "Landslide",
                Kanji = "崩",
                Element = Element.Earth,
                Cost = 0,
                Target = CardTarget.Yokai,
                SFX= "Wow",
                Description = "Rotate the sealing circle counter-clockwise\n",
                Use = async (useLocation) => {
                var lastElement = BattleScene.SealSlots[0];
                     List<Task> tasks = new List<Task>();
                    for (int i = 0 ; i < BattleScene.SealCount - 1; i++) {
                        tasks.Add(BattleScene.SealingCircle.MoveSeal(i+1, i, BattleScene.SealSlots[i+1]));
                        BattleScene.SealSlots[i]= BattleScene.SealSlots[i+1];
                    }
                    tasks.Add( BattleScene.SealingCircle.MoveSeal(0, BattleScene.SealSlots.Count -1, lastElement));
                    foreach (Task task in tasks) {
                        await task;
                    }
                    BattleScene.SealSlots[BattleScene.SealSlots.Count -1] = lastElement;
                    BattleScene.SealingCircle.DisplaySeals();
                }
            },
            new CardData {Id = CardId.Carving,
                Name = "Carving",
                Kanji = "彫",
                Element = Element.Earth,
                Cost = 1,
                SFX = "PlaceSeal",
                Target = new CardTarget{ TargetDescription = "An [empty-seal] adjacent to an [earth-seal]",
                    CheckTargetableFunc = (int loc) => { return (loc == -1) ? false : BattleScene.SealSlots[CardEffectHelper.NextLocation(loc)] == Element.Earth ||
                    BattleScene.SealSlots[CardEffectHelper.PrevLocation(loc)] == Element.Earth; }
                },
                Description = "Place an [earth-seal] adjacent to another [earth-seal] without swapping the surrounding Seals",
                Use = async (useLocation) => {
                     await BattleScene.Instance.SwitchSeal(Element.Earth, useLocation);
                }
            },
            new CardData {Id = CardId.Tectonic,
                Name = "Tectonic",
                Kanji = "地",
                Element = Element.Earth,
                Cost = 0,
                SFX = "Yo",
                Target = CardTarget.NonEmptySeal,
                Description = "Remove a Seal and turn it into a one-time use card that costs 0 [ki]",
                Use = async (useLocation) => {
                     var getCard = BattleScene.SealSlots[useLocation] switch {
                            Element.Fire => CardId.FreeFire,
                            Element.Water => CardId.FreeWater,
                            Element.Wood => CardId.FreeWood,
                            Element.Earth => CardId.FreeEarth,
                            Element.Metal => CardId.FreeMetal,
                            _ => CardId.Tectonic,
                        };
                    Task task = BattleScene.Instance.RemoveSeal(useLocation);
                    await BattleScene.Hand.AddCard(getCard);
                    await task;
                }
            },


            new CardData {
                Id = CardId.FreeFire,
                Name = "Fire Seal",
                Kanji = "火",
                Element = Element.Fire,
                Cost = 0,
                SFX = "TaikoLarge",
                BanishAfterUse = true,
                Description = "Click on a slot to place a [fire-seal]\nBanish this card",
                Use =  async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Fire, useLocation);}
            },
            new CardData {
                Id = CardId.FreeWater,
                Name = "Water Seal",
                Kanji = "水",
                SFX = "TaikoFunky",
                Element = Element.Water,
                Cost = 0,
                BanishAfterUse = true,
                Description = "Click on a slot to place a [metal-seal]\nBanish this card",
                Use =  async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Water, useLocation); }
            },
            new CardData {
                Id = CardId.FreeWood,
                Name = "Wood Seal",
                Kanji = "木",
                Element = Element.Wood,
                Cost = 0,
                SFX = "TaikoMedium",
                BanishAfterUse = true,
                Description = "Click on a slot to place a [wood-seal] and gain one Seed\nBanish this card",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal (Element.Wood, useLocation); await BattleScene.AddSeeds(1); }
            },
            new CardData {
                Id = CardId.FreeEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                SFX = "PlaceSeal",
                Cost = 0,
                BanishAfterUse = true,
                Description = "Click on a slot to place a [earth-seal]\nBanish this card",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Earth, useLocation); }
            },
            new CardData {Id = CardId.FreeMetal, // Metal becomes en metal
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 0,
                SFX = "TaikoSmall",
                BanishAfterUse = true,
                Description = "Click on a slot to place a [metal-seal]\nBanish this card",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Metal, useLocation); }
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

