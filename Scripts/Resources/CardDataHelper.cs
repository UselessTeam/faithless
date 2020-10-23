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

    Flood,
    Watermill,
    HotSpring,
    Tsunami,
    WashAway,

    Recycle,
    Forge,
    SteelTools,
    Stronghold,
    Rust,

    PineCone,
    Abundance,
    Roots,
    Plantation,
    RiceField,

    Eruption,
    Combustion,
    Phoenix,
    FireSpread,
    Cooking,

    Drought,
    Landslide,
    Carving,
    Tectonic,
    TOTAL, // Leave at the end
    // Add Free seal
}

public static class CardIdExtensions {
    public static CardData Data (this CardId id) {
        return CardData.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }


public class CardTarget {
    public string TargetDescription;
    public bool TargetYokai = false;
    public Func<int, bool> CheckTargetableFunc = (int id) => { return false; };

    public static CardTarget Yokai = new CardTarget {
        TargetDescription = "Yokai",
        TargetYokai = true,
    };
    public static CardTarget AnySeal = new CardTarget {
        TargetDescription = "Any Location",
        CheckTargetableFunc = (int id) => { return true; }
    };
    public static CardTarget EmptySeal = new CardTarget {
        TargetDescription = "Empty Seal Location",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.None; }
    };
    public static CardTarget NonEmptySeal = new CardTarget {
        TargetDescription = "Any Non-Empty Seal",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] != Element.None; }
    };
    public static CardTarget FireSeal = new CardTarget {
        TargetDescription = "[fire-seal]",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.Fire; }
    };
    public static CardTarget WaterSeal = new CardTarget {
        TargetDescription = "[water-seal]",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.Water; }
    };
    public static CardTarget WoodSeal = new CardTarget {
        TargetDescription = "[wood-seal]",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.Wood; }
    };
    public static CardTarget EarthSeal = new CardTarget {
        TargetDescription = "[earth-seal]",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.Earth; }
    };
    public static CardTarget MetalSeal = new CardTarget {
        TargetDescription = "[metal-seal]",
        CheckTargetableFunc = (int id) => { return BattleScene.SealSlots[id] == Element.Metal; }
    };
}