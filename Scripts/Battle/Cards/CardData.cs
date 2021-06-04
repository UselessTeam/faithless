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
    public string Description = "This is an empty talisman";
    public Func<int, Task> Use = (int id) => { GD.PrintErr("This talisman does nothing"); return null; };
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
                Description = "Place a [fire-seal] on the selected slot. \nAt the start of your turn, a [fire-seal] [?ignite]ignites[/?] any surrounding [wood-seal], turning them into [fire-seals] and producing 1 [ki]",
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
                Description = "Place a [water-seal] on the selected slot. \nIf a Yokai tries to remove this Seal, it turns into an [earth-seal] instead. \nAt the end of your turn, adjacent [wood-seals] can [?harvest]harvest[/?]",
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
                Description = "Place a [wood-seal] on the selected slot. \nGain one [seed]",
                Use = async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal (Element.Wood, useLocation);
                    await BattleScene.AddSeeds(1);}
            },
            new CardData {                Id = CardId.BasicEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                Cost = 2,
                SFX = "PlaceSeal",
                Description = "Place a [earth-seal] on the selected slot and swap the two neighouring Seals. \nIf an [earth-seal] would be swapped, swap the next Seal instead. \n[?trigger-effect]Trigger the effects[/?] of the Seals you swapped",
                Use = async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal(Element.Earth, useLocation);

                    int locationBefore = useLocation;
                    do{
                        locationBefore = locationBefore.PrevLocation();
                        if(locationBefore == useLocation)
                            return;
                    }while (BattleScene.SealSlots[locationBefore] == Element.Earth);
                    int locationAfter = useLocation;
                    do{
                        locationAfter = locationAfter.NextLocation();
                    }while (BattleScene.SealSlots[locationAfter] == Element.Earth);

                    await BattleScene.Instance.SwapSeals(locationBefore, locationAfter);
                    Task task = CardEffectHelper.TriggerEffect(locationAfter);
                    if( locationBefore != locationAfter) await CardEffectHelper.TriggerEffect(locationBefore);
                    await task;
                }
            },
            new CardData {Id = CardId.BasicMetal, // Metal becomes en metal
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 2,
                SFX = "TaikoSmall",
                Description = "Place a [metal-seal] on the selected slot. \nWhen a Yokai attacks this Seal, they become [?stagger]staggered[/?] for the next turn, and this Seal turns into an [earth-seal]",
                Use = async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal(Element.Metal, useLocation); }
            },


            new CardData {Id = CardId.Flood,
                Name = "Flood",
                Kanji = "洪",
                Element = Element.Water,
                Cost = 0,
                Target = CardTarget.EarthSeal,
                SFX = "TaikoFunky",
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
                Description = "Discard all your talismans. \nDraw as many talismans + 1",
                Use = async (useLocation) => {
                    int cardCount =  BattleScene.Cards.Hand.Count() + 1;
                    await BattleScene.Cards.DiscardHand(true);
                    await BattleScene.DrawCards(cardCount);
                }
            },
            new CardData {Id = CardId.Tides,
                Name = "Tides",
                Kanji = "潮",
                Element = Element.Water,
                Cost = 2,
                Target = CardTarget.Yokai,
                SFX = "GenericEffect",
                Description = "Do not discard your hand at the end of this turn",
                Use = async (useLocation) => {
                    BattleScene.Instance.RetainHand = true;
                }
            },
            new CardData {Id = CardId.Tsunami,
                Name = "Tsunami",
                Kanji = "波",
                Element = Element.Water,
                Cost = 3,
                SFX = "YoLong",
                BanishAfterUse = true,
                Target = CardTarget.EmptySeal,
                Description = "Target an empty area. \nDiscard all your talismans. \nPlace one [water-seal] in the area for each talisman you discarded. \nBanish this talisman until the end of the combat",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    var area = CardEffectHelper.GetArea(useLocation, Element.None);
                    var cardCount = BattleScene.Cards.Hand.Count();
                    tasks.Add(BattleScene.Cards.DiscardHand(true));

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
            /* new CardData {Id = CardId.Watermill,
                Name = "Watermill",
                Kanji = "津",
                Element = Element.Water,
                Cost = 0,
                Target = CardTarget.WaterSeal,
                SFX = "Wow",
                Description = "Replace a [water-seal] by a random other element. \nGain 1 [ki]",
                Use = async (useLocation) => {
                     var elmIdx = Utils.RNG.rng.Next(0,4);
                    await BattleScene.Instance.SwitchSeal(elmIdx switch {
                            0 => Element.Fire,
                            1 => Element.Wood,
                            2 => Element.Earth,
                            _ => Element.Metal,
                        }, useLocation
                    );
                    //if (BattleScene.SealSlots[useLocation] == Element.Earth) 
                    BattleScene.Ki +=1;

                }
            }, */
            new CardData {Id = CardId.Irrigation,
                Name = "Irrigation",
                Kanji = "灌",
                Element = Element.Water,
                Cost = 0,
                Target = CardTarget.Yokai,
                SFX = "Wow",
                Description = "Start a [?harvest]harvest[/?] on all [wood-seals] adjacent to a [water-seal]",
                Use = async (useLocation) => {
                    await BattleScene.Instance.TriggerHarvest();
                }
            },
            new CardData {Id = CardId.HotSpring, // Too much use with fire
                Name = "Hot Spring",
                Kanji = "泉",
                Element = Element.Water,
                Cost = 2,
                SFX = "TaikoFunky",
                Description = "Place a [water-seal]. \nGain 1 health if the target location contained a [fire-seal]. \nGain 1 health for each [fire-seal] adjacent to the target location",
                Use = async (useLocation) => {
                    if (BattleScene.SealSlots[useLocation] == Element.Fire) BattleScene.Health += 1;
                    int sealCount =  BattleScene.SealSlots.Count;
                    await BattleScene.Instance.PlaceSeal(Element.Water, useLocation);
                    if (BattleScene.SealSlots[useLocation.NextLocation()] == Element.Fire) BattleScene.Health += 1;
                    if (BattleScene.SealSlots[useLocation.PrevLocation()] == Element.Fire) BattleScene.Health += 1;
                }
            },


            new CardData {Id = CardId.Armaments,
                Name = "Armaments",
                Kanji = "軍",
                Element = Element.Metal,
                Cost = 1,
                Target = CardTarget.Yokai,
                Description = "Replace all [earth-seals] by [metal-seals]",
                SFX = "TaikoSmall",
                Use = async (useLocation) => {
                     for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Earth) { await BattleScene.Instance.PlaceSeal(Element.Metal, i); }
                    }
                }
            },
            new CardData {Id = CardId.Forge,
                Name = "Forge",
                Kanji = "鍛",
                Element = Element.Metal,
                Cost = 3,
                SFX = "TaikoSmall",
                Description = "Place a [metal-seal] on the selected slot, and another one on the opposite side of the board",
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
                Description = "The [?harvest]harvest[/?] effect of [wood-seals] grants one more card. \nBanish this talisman until the end of the combat",
                Use = async (useLocation) => {
                     BattleScene.Instance.HarvestBonus +=1;
                }
            },
            new CardData {Id = CardId.Stronghold,
                Name = "Stronghold",
                Kanji = "砦",
                Element = Element.Metal,
                Cost = 2,
                SFX = "TaikoSmall",
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
                SFX = "TaikoFunky",
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
                Description = "Discard as many talismans from your hand as you want. \nGain 1 [seed] for each talisman discarded. \nOnce you are done, you can discard this talisman and gain 1 [seed]",
                Use = async (useLocation) => {
                    while (true){
                        BattleMessages.OpenDiscardMessage();
                        int discard = await BattleScene.Cards.StartCardInput();;
                        await BattleScene.AddSeeds(1);
                        if(BattleScene.Cards.IsSelected(discard))
                            break;
                        else
                            await BattleScene.Cards.DiscardCard(discard);
                    }
                    BattleMessages.CloseDiscardMessage();
                    // await BattleScene.Cards.DiscardHand(true);
                }
            },
            new CardData {Id = CardId.Roots,
                Name = "Roots",
                Kanji = "根",
                Element = Element.Wood,
                Cost = 0,
                SFX = "Wow",
                Target = CardTarget.Yokai,
                Description = "Draw the last talisman you discarded (if the discard is empty, draw a talisman form your deck instead). \nIf you drew a Water or Earth talisman, gain one [seed]",
                Use = async (useLocation) => {
                     if(!await BattleScene.Cards.DrawLastDiscard()) {
                        await BattleScene.Cards.DrawCard();
                    }
                    var lastDrawn = BattleScene.Cards.HandAt(BattleScene.Cards.Hand.Count() -1).Data();
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
                SFX = "TaikoMedium",
                Target = CardTarget.EarthSeal,
                Description = "Replace a group of continuous [earth-seals] by [wood-seals]. \nGain one [seed] per Seal replaced",
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
                Target = new TargetAdjacent(CardTarget.WaterSeal),
                SFX = "TaikoMedium",
                Description = "Target a location adjacent to a [water-seal] and place a [wood-seal]",
                Use = async (useLocation) => {
                    await BattleScene.Instance.PlaceSeal(Element.Wood, useLocation);
                }
            },


            new CardData {Id = CardId.Smelt,
                Name = "Smelt",
                Kanji = "溶",
                Element = Element.Fire,
                Cost = 0,
                Target = CardTarget.NonEmptySeal,
                SFX = "Yo",
                Description = "Remove a seal of any type. \nIf you remove a [metal-seal], you can play a talisman for free. \nElse, you can play a talisman of the same type for free",
                Use = async (useLocation) => {
                    Element element = BattleScene.SealSlots[useLocation];
                    BattleScene.Instance.NextCardFree[ (element==Element.Metal)? Element.None : element] = true;
                    await BattleScene.Instance.RemoveSeal(useLocation);
                }
            },
            new CardData {Id = CardId.Furnace,
                Name = "Furnace",
                Kanji = "窯",
                Element = Element.Fire,
                Cost = 0,
                Target = new TargetOr( CardTarget.WoodSeal, CardTarget.FireSeal),
                SFX = "Wow",
                Description = "Remove a [wood-seal] or [fire-seal]. \nDraw 1 talisman and gain 2 [ki]",
                Use = async (useLocation) => {
                     Task task = BattleScene.Instance.RemoveSeal(useLocation);
                    BattleScene.Ki += 2;
                    await BattleScene.DrawCards(1);
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
                Description = "Select one [fire-seal]. \nPlace a [fire-seal] on surrounding locations",
                Use = async (useLocation) => {
                     Task t = BattleScene.Instance.PlaceSeal(Element.Fire, CardEffectHelper.NextLocation(useLocation));
                    await BattleScene.Instance.PlaceSeal(Element.Fire, CardEffectHelper.PrevLocation(useLocation));
                    await t;
                }
            },
            /* new CardData {Id = CardId.Phoenix,
                Name = "Phoenix",
                Kanji = "鵬",
                Element = Element.Fire,
                Cost = 4,
                Target = CardTarget.Yokai,
                SFX = "SuspensePercussion",
                Description = "Destroy all Seals. \nGain 2 [ki] for each Seal destroyed. \nDiscard all talismans. \nDraw talismans (as a new turn). \nHeal to full health",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if(BattleScene.SealSlots[i] != Element.None){
                            tasks.Add(BattleScene.Instance.RemoveSeal(i));
                            BattleScene.Ki += 2;
                        }
                    }
                    await BattleScene.Hand.DiscardHand(true);
                    foreach(var task in tasks) await task;
                    await BattleScene.DrawCards(GameData.Instance.CardsPerTurn);
                    BattleScene.Health = GameData.Instance.MaxHealth;
                }
            }, */
            new CardData {Id = CardId.Cooking,
                Name = "Cooking",
                Kanji = "料",
                Element = Element.Fire,
                Cost = 0,
                Target = CardTarget.Yokai,
                SFX = "GenericEffect",
                Description = "Remove all [seeds]. \nDraw 1 talisman for each seed removed",
                Use = async (useLocation) => {
                    await BattleScene.DrawCards(BattleScene.Seeds);
                    await BattleScene.AddSeeds(-BattleScene.Seeds);
                }
            },
            new CardData {Id = CardId.Incineration,
                Name = "Incineration",
                Kanji = "炎",
                Element = Element.Fire,
                Cost = 1,
                SFX = "SuspensePercussion",
                Target = CardTarget.Yokai,
                Description = "On his next turn, each time the Yokai attacks or removes a [fire-seal], he will get staggered",
                Use = async (useLocation) => {
                    BattleScene.YokaiAI.Incinerate = true;
                }
            },
            new CardData {Id = CardId.Volcano,
                Name = "Volcano",
                Kanji = "噴",
                Element = Element.Fire,
                Cost = 3,
                SFX = "SuspensePercussion",
                Target = CardTarget.EarthSeal,
                Description = "Select an [earth-seal] and replace it with a [fire-seal]. \n[?push]Push[/?] adjacent seals and place 2 [earth-seals] around the selected location",
                Use = async (useLocation) => {
                    List<Task> tasks= new List<Task>();
                    tasks.Add( CardEffectHelper.Push(useLocation.NextLocation(), true, false));
                    tasks.Add( CardEffectHelper.Push(useLocation.PrevLocation(), false, false));
                    tasks.Add( BattleScene.Instance.PlaceSeal(Element.Fire, useLocation));
                    await Task.WhenAll(tasks);
                    BattleScene.SealingCircle.DisplaySeals();

                    tasks= new List<Task>();
                    if( BattleScene.SealSlots[ useLocation.NextLocation()] == Element.None)
                        tasks.Add(BattleScene.Instance.PlaceSeal(Element.Earth, useLocation.NextLocation()));
                    if( BattleScene.SealSlots[ useLocation.PrevLocation()] == Element.None)
                        tasks.Add( BattleScene.Instance.PlaceSeal(Element.Earth, useLocation.PrevLocation()));
                    await Task.WhenAll(tasks);

                }
            },


            new CardData {Id = CardId.Drought,
                Name = "Drought",
                Kanji = "旱",
                Element = Element.Earth,
                Cost = 2,
                Target  = CardTarget.Yokai,
                SFX= "PlaceSeal",
                Description = "Replace all [water-seal] and [wood-seal] by [earth-seal]. \nGain 1 [ki] for each [water-seal] replaced. \nDraw 1 talisman for each [wood-seal] replaced. \nOne use per turn",
                Use = async (useLocation) => {
                     List<Task> tasks = new List<Task>();
                    for (int i = 0 ; i < BattleScene.SealSlots.Count ; i++) {
                        if (BattleScene.SealSlots[i] == Element.Water) { tasks.Add(BattleScene.Instance.PlaceSeal(Element.Earth, i)); BattleScene.Ki += 1; }
                        if (BattleScene.SealSlots[i] == Element.Wood) { tasks.Add(BattleScene.Instance.PlaceSeal(Element.Earth, i));  tasks.Add(BattleScene.DrawCards(1));}
                    }
                    foreach(var task in tasks) await task;
                }
            },
            new CardData {Id = CardId.Landslide,
                Name = "Landslide",
                Kanji = "崩",
                Element = Element.Earth,
                Cost = 0,
                Target = CardTarget.NonEmptySeal,
                SFX= "Yo",
                Description = "Select a Seal and [?push]push[/?] it counter-clockwise. \nOr select the center and [?push]push[/?] all Seals counter-clockwise",
                Use = async (useLocation) => {
                    if (useLocation == -1)
                        await CardEffectHelper.Push(useLocation, false, true);
                    else{
                        await CardEffectHelper.RotateCircle(false);
                    }
                    BattleScene.SealingCircle.DisplaySeals(); //Sanity check
                }
            },
            new CardData {Id = CardId.Tectonic,
                Name = "Tectonic",
                Kanji = "地",
                Element = Element.Earth,
                Cost = 1,
                SFX = "PlaceSeal",
                Target = new TargetAdjacent(CardTarget.EarthSeal),
                Description = "Place an [earth-seal] adjacent to another [earth-seal]. \nIf there is a Seal on the selected location, it is pushed away",
                Use = async (useLocation) => {
                    Element element = BattleScene.SealSlots[useLocation];
                    if(element!= Element.None){
                        bool clockwise = (Utils.RNG.rng.Next()%2 == 0);

                        if( BattleScene.SealSlots[ useLocation.AdjacentLocation(clockwise) ] != Element.Earth){
                            await CardEffectHelper.Push(useLocation, clockwise, false);
                        }else
                            await CardEffectHelper.Push(useLocation, !clockwise, false);
                    }
                    await BattleScene.Instance.PlaceSeal(Element.Earth, useLocation);
                }
            },
            new CardData {Id = CardId.TradeRoute,
                Name = "Trade Route",
                Kanji = "輸",
                Element = Element.Earth,
                Cost = 2,
                SFX = "GenericEffect",
                Target = CardTarget.AnySeal,
                Description = "Place an [earth-seal]. \nIf the target location already contains Seal, turn it into a one-time-use free talisman",
                Use = async (useLocation) => {
                     var getCard = BattleScene.SealSlots[useLocation] switch {
                            Element.Fire => CardId.FreeFire,
                            Element.Water => CardId.FreeWater,
                            Element.Wood => CardId.FreeWood,
                            Element.Earth => CardId.FreeEarth,
                            Element.Metal => CardId.FreeMetal,
                            _ => CardId.None,
                        };
                    Task task = BattleScene.Instance.PlaceSeal(Element.Earth, useLocation);
                    if(getCard != CardId.None)
                        await BattleScene.Cards.GetCard(getCard);
                    await task;
                }
            },
            new CardData {Id = CardId.Earthquake,
                Name = "Earthquake",
                Kanji = "震",
                Element = Element.Earth,
                Cost = 2,
                SFX = "YoLong",
                Target = CardTarget.Yokai,
                Description = "Randomly reorganise the seals on the field. \nThe Yokai is greatly staggered",
                Use = async (useLocation) => {
                    List<Task> tasks = new List<Task>();
                    bool[] chosenSeals = Enumerable.Repeat(false, BattleScene.SealCount).ToArray();
                    for (int i = 0; i < BattleScene.SealCount / 2; i++){
                        int firstSeal= 0;
                        int secondSeal= 0;
                        while(chosenSeals[firstSeal])
                            firstSeal = Utils.RNG.rng.Next(0, BattleScene.SealCount);
                        chosenSeals[firstSeal] = true;
                        while(chosenSeals[secondSeal])
                            secondSeal = Utils.RNG.rng.Next(0, BattleScene.SealCount);
                        chosenSeals[secondSeal] = true;
                        tasks.Add(BattleScene.Instance.SwapSeals(firstSeal, secondSeal));
                    }
                    BattleScene.YokaiAI.StaggerLevel += 2;
                    BattleScene.YokaiAI.PlanNextTurn();
                    foreach(var task in tasks)
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
                Description = "Click on a slot to place a [fire-seal]. \nBanish this talisman",
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
                Description = "Click on a slot to place a [metal-seal]. \nBanish this talisman",
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
                Description = "Click on a slot to place a [wood-seal] and gain one Seed. \nBanish this talisman",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal (Element.Wood, useLocation); }
            },
            new CardData {
                Id = CardId.FreeEarth,
                Name = "Earth Seal",
                Kanji = "土",
                Element = Element.Earth,
                SFX = "PlaceSeal",
                Cost = 0,
                BanishAfterUse = true,
                Description = "Click on a slot to place a [earth-seal]. \nBanish this talisman",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Earth, useLocation); }
            },
            new CardData {Id = CardId.FreeMetal, // Metal becomes en metal
                Name = "Metal Seal",
                Kanji = "金",
                Element = Element.Metal,
                Cost = 0,
                SFX = "TaikoSmall",
                BanishAfterUse = true,
                Description = "Click on a slot to place a [metal-seal]. \nBanish this talisman",
                Use = async (useLocation) => {   await BattleScene.Instance.PlaceSeal(Element.Metal, useLocation); }
            },


        }).ToDictionary((card) => card?.Id ?? CardId.None);

    public static CardData Find (CardId id) {
        try {
            return list[id];
        } catch {
            GD.PrintErr($"Trying to access a non-existing talisman {id}");
            return null;
        }
    }
    public static bool CheckPlayable (CardId id, int location) {
        return id.Data().Target.CheckTargetableFunc(location);
    }
}

