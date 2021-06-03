using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public enum YokaiActionType {
    None,
    Attack,
    Remove,
    AttackPierce,
    AttackOrRemove
}

public struct YokaiAction {
    public YokaiActionType Type;
    public bool IsBlocked;
    public YokaiAction (YokaiActionType type, bool blocked = false) {
        Type = type;
        IsBlocked = blocked;
    }
}

public static class YokaiActionExtention {
    // public static List<YokaiAction> YokaiActionList = new List<YokaiAction>() {
    //     YokaiAction.Attack, YokaiAction.Remove, YokaiAction.Remove, YokaiAction.AttackOrRemove, YokaiAction.Remove, YokaiAction.Attack, YokaiAction.Remove,
    // };

    public static string Description (this YokaiAction action) {
        if (action.IsBlocked)
            return "The Yokai is staggered !\nThis action is rendered unusable for one turn";
        else
            return action.Type
            switch {
                YokaiActionType.Attack => "Attack\n\nThe Yokai will attack this location and remove 1 health\nYou can defend yourself by placing a Seal of any type",
                YokaiActionType.AttackOrRemove => "Attack and Remove\n\nThe Yokai will attack, then remove the seal in this location",
                YokaiActionType.AttackPierce => "Pierce Attack\n\nRemove one health",
                YokaiActionType.Remove => "Remove\n\nThe Yokai will remove the seal in this location",
                _ => "ERROR"
            };
    }

    public static async Task Perform (this YokaiAI yokai, YokaiAction action, int i) {
        if (action.IsBlocked) return;

        switch (action.Type) {
            case YokaiActionType.Attack:
            case YokaiActionType.AttackPierce:
                await yokai.AttackOn(i);
                break;
            case YokaiActionType.AttackOrRemove:
                await yokai.AttackOn(i);
                goto case YokaiActionType.Remove;
            case YokaiActionType.Remove:
                if (BattleScene.SealSlots[i] == Element.Water) {
                    BattleScene.Instance.LogPanel.Log($"The Yokai tries to remove a [water-seal], but it turns into an [earth-seal]");
                    await BattleScene.Instance.PlaceSeal(Element.Earth, i);
                } else if (yokai.Incinerate && BattleScene.SealSlots[i] == Element.Fire) {
                    yokai.StaggerLevel += 1;
                    await BattleScene.Instance.RemoveSeal(i);
                } else
                    await BattleScene.Instance.RemoveSeal(i);
                break;
        }
    }

    static async Task<bool> AttackOn (this YokaiAI yokai, int i) {
        if (BattleScene.SealSlots[i] == Element.Metal) {
            BattleScene.Instance.LogPanel.Log($"Your [metal-seal] staggers the Yokai, and turns into an [earth-seal]");
            yokai.StaggerLevel += 1;
            await BattleScene.Instance.PlaceSeal(Element.Earth, i);
            return false;
        } else if (yokai.Incinerate && BattleScene.SealSlots[i] == Element.Fire) {
            yokai.StaggerLevel += 1;
            return false;
        } else if (BattleScene.SealSlots[i] != Element.None) {
            return false;
        }
        BattleScene.Instance.LogPanel.Log($"The Yokai attacks you for 1 damage");
        BattleScene.Health -= 1;
        return true;
    }

}