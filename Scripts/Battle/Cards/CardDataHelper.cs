using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public enum CardId {
    None,
    BasicEarth,
    BasicFire,
    BasicWater,
    BasicWood,
    BasicMetal,

    Drought,
    Landslide,
    TradeRoute,
    Tectonic,
    Earthquake,

    Volcano,
    Furnace,
    //Phoenix,
    Cooking,
    FireSpread,
    Incineration,
    Smelt,

    Flood,
    //Watermill,
    Irrigation,
    HotSpring,
    Tsunami,
    WashAway,
    Tides,

    PineCone,
    Abundance,
    Roots,
    Plantation,
    RiceField,

    Armaments,
    Forge,
    SteelTools,
    Stronghold,
    Rust,
    TOTAL, // Leave at the end

    // Cards created in combat
    FreeEarth,
    FreeFire,
    FreeWater,
    FreeWood,
    FreeMetal,
}

public static class CardEffectHelper {
    public static Tuple<int, int> GetArea (int loc, Element element) {
        int i = loc;
        while (BattleScene.SealSlots[PrevLocation(i)] == element) {
            i = PrevLocation(i);
            if (i == loc) return new Tuple<int, int>(loc, loc);
        }
        int j = loc;
        while (BattleScene.SealSlots[j] == element) {
            j = NextLocation(j);
        }
        return new Tuple<int, int>(i, j);
    }

    public static Element GetSeal (this int loc) { return BattleScene.SealSlots[loc]; }

    public static int NextLocation (this int loc) { return (loc + 1) % BattleScene.SealCount; }
    public static int PrevLocation (this int loc) { return (loc + BattleScene.SealCount - 1) % BattleScene.SealCount; }
    public static int AdjacentLocation (this int loc, bool clockwise) { return (clockwise) ? NextLocation(loc) : PrevLocation(loc); }

    public static List<int> GetNonEmptySeals () {
        var nonempty = new List<int>();
        for (int i = 0 ; i < BattleScene.SealCount ; i++)
            if (BattleScene.SealSlots[i] != Element.None) nonempty.Add(i);
        return nonempty;
    }
    public static int NonEmptySealsCount () {
        return NonEmptySealsCount(0, BattleScene.SealCount);
    }
    public static int NonEmptySealsCount (int start, int end) {
        int count = 0;
        for (int i = start ; i < end ; i++)
            count += (BattleScene.SealSlots[i] == Element.None) ? 0 : 1;
        return count;
    }

    public static Task Push (int useLocation, bool clockwise, bool forcePush = false) {
        if (BattleScene.SealSlots[useLocation] == Element.None)
            return Task.Delay(0); //Nothing to push empty

        int pushUntil = AdjacentLocation(useLocation, clockwise);
        while (BattleScene.SealSlots[pushUntil] != Element.None && pushUntil != useLocation) {
            pushUntil = AdjacentLocation(pushUntil, clockwise);
        }

        if (!forcePush && pushUntil == useLocation)
            return Task.Delay(0); //Don't push if the board is already full
        else if (forcePush && pushUntil == useLocation)
            return RotateCircle(clockwise);

        var selectedElement = BattleScene.SealSlots[useLocation];
        List<Task> tasks = new List<Task>();
        int i = pushUntil;
        while (i != useLocation) {
            int j = AdjacentLocation(i, !clockwise);
            tasks.Add(BattleScene.SealingCircle.MoveSeal(j, i, BattleScene.SealSlots[j]));
            BattleScene.SealSlots[i] = BattleScene.SealSlots[j];
            i = j;
        }
        BattleScene.SealSlots[useLocation] = Element.None;
        return Task.WhenAll(tasks);
    }

    public static Task RotateCircle (bool clockwise) {
        var lastElement = BattleScene.SealSlots[0];
        List<Task> tasks = new List<Task>();
        int i = 0;
        while (AdjacentLocation(i, clockwise) != 0) {
            int j = AdjacentLocation(i, !clockwise);
            tasks.Add(BattleScene.SealingCircle.MoveSeal(j, i, BattleScene.SealSlots[j]));
            BattleScene.SealSlots[i] = BattleScene.SealSlots[j];
            i = j;
        }
        tasks.Add(BattleScene.SealingCircle.MoveSeal(0, i.AdjacentLocation(clockwise), lastElement));
        BattleScene.SealSlots[i.AdjacentLocation(clockwise)] = lastElement;
        return Task.WhenAll(tasks);
    }

    public static async Task TriggerEffect (int location) {
        switch (location.GetSeal()) {
            case Element.Fire:
                foreach (var adjLoc in new int[2] { location.NextLocation(), location.PrevLocation() })
                    if (adjLoc.GetSeal() == Element.Wood) {
                        await BattleScene.Instance.PlaceSeal(Element.Fire, adjLoc);
                        BattleScene.Ki += 1;
                    }
                break;
            case Element.Wood:
                await BattleScene.AddSeeds(1);
                break;
            case Element.Water:
                foreach (var adjLoc in new int[2] { location.NextLocation(), location.PrevLocation() })
                    if (adjLoc.GetSeal() == Element.Wood) {
                        await BattleScene.DrawCards(1);
                    }
                break;
        }
    }
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
