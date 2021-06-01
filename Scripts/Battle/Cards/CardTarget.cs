using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

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

public class TargetOr : CardTarget {
    CardTarget A;
    CardTarget B;
    public TargetOr (CardTarget a, CardTarget b) {
        this.A = a;
        this.B = b;
        TargetDescription = A.TargetDescription + " or " + B.TargetDescription;
        CheckTargetableFunc = (int id) => A.CheckTargetableFunc(id) || B.CheckTargetableFunc(id);
    }
}

public class TargetAdjacent : CardTarget {
    CardTarget A;
    public TargetAdjacent (CardTarget a) {
        this.A = a;
        TargetDescription = "Adjacent to a " + A.TargetDescription;
        CheckTargetableFunc = (int id) => id >= 0 &&
                (A.CheckTargetableFunc(id.NextLocation()) || A.CheckTargetableFunc(id.PrevLocation()));
    }
}