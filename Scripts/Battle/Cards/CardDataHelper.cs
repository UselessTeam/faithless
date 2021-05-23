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

    // Cards created in combat
    FreeFire,
    FreeWater,
    FreeWood,
    FreeEarth,
    FreeMetal,

}

public static class CardEffectHelper {
    public static Tuple<int, int> GetArea (int loc, Element element) {
        int i = loc;
        while (BattleScene.SealSlots[PrevLocation(i)] == element) {
            i = PrevLocation(i);
            if (i == loc) return new Tuple<int, int>(NextLocation(loc), loc);
        }
        int j = loc;
        while (BattleScene.SealSlots[j] == element) {
            j = NextLocation(j);
        }
        return new Tuple<int, int>(i, j);
    }

    public static int NextLocation (int loc) { return (loc + 1) % BattleScene.SealCount; }
    public static int PrevLocation (int loc) { return (loc + BattleScene.SealCount - 1) % BattleScene.SealCount; }

}

public static class CardIdExtensions {
    public static CardData Data (this CardId id) {
        return CardData.Find(id);
    }
}

public enum Element { None, Fire, Water, Wood, Earth, Metal }

public static class ElementExtensions {
    public static string Description (this Element element) => element switch {
        Element.Fire => "[fire-seal]\n\nAt the start of your turn, a [fire-seal] [?ignite]ignites[/?] any surrounding [wood-seal], turning them into [fire-seals], and producing 1 [ki]",
        Element.Wood => "[wood-seal]\n\nAt the end of your turn, a [wood-seal] next to at least one [water-seal] [?harvest]harvests[/?] (draws 1 card)",
        Element.Water => "[water-seal]\n\nIf an enemy tries to remove this Seal, it will turn into an [earth-seal] instead",
        Element.Metal => "[metal-seal]\n\nWhen a Yokai attacks this Seal, he becomes [?stagger]staggered[/?] for the next turn, and this Seal turns into an [earth-seal]",
        Element.Earth => "[earth-seal]\n\nThis has no particular effect by itself",
        _ => "",
    };
}

public class CardTarget {
    public string TargetDescription;
    public Func<int, bool> CheckTargetableFunc = (int id) => { return false; };

    public static CardTarget Yokai = new CardTarget {
        TargetDescription = "Yokai",
        CheckTargetableFunc = (int id) => { return (id == -1) ? true : false; }
    };
    public static CardTarget AnySeal = new CardTarget {
        TargetDescription = "Any Location",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : true; }
    };
    public static CardTarget EmptySeal = new CardTarget {
        TargetDescription = "Empty Seal Location",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.None; }
    };
    public static CardTarget NonEmptySeal = new CardTarget {
        TargetDescription = "Any Non-Empty Seal",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] != Element.None; }
    };
    public static CardTarget FireSeal = new CardTarget {
        TargetDescription = "[fire-seal]",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.Fire; }
    };
    public static CardTarget WaterSeal = new CardTarget {
        TargetDescription = "[water-seal]",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.Water; }
    };
    public static CardTarget WoodSeal = new CardTarget {
        TargetDescription = "[wood-seal]",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.Wood; }
    };
    public static CardTarget EarthSeal = new CardTarget {
        TargetDescription = "[earth-seal]",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.Earth; }
    };
    public static CardTarget MetalSeal = new CardTarget {
        TargetDescription = "[metal-seal]",
        CheckTargetableFunc = (int id) => { return (id == -1) ? false : BattleScene.SealSlots[id] == Element.Metal; }
    };
}