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
    AttackAndRemove,
    ElementalAttack, //Like an attack and remove, but with element
}

public struct YokaiAction {
    public YokaiActionType Type;
    public Element Element;

    public bool IsBlocked;
    public YokaiAction (YokaiActionType type, bool blocked = false) {
        Type = type;
        Element = Element.None;
        IsBlocked = blocked;
    }
    public YokaiAction (YokaiActionType type, Element element, bool blocked = false) {
        if (type == YokaiActionType.ElementalAttack && element == Element.None)
            type = YokaiActionType.AttackAndRemove;
        Type = type;
        Element = (type == YokaiActionType.ElementalAttack) ? element : Element.None;
        IsBlocked = blocked;
    }
    public YokaiAction (Element element = Element.None, bool blocked = false) {
        Type = YokaiActionType.ElementalAttack;
        Element = element;
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
        else {
            return action.Type
        switch {
            YokaiActionType.Attack => "Attack\n\nThe Yokai will attack this location and remove 1 health.\nYou can defend yourself by placing a Seal of any type",
            YokaiActionType.AttackAndRemove => "Attack and Remove\n\nThe Yokai will attack\nAfterwards, the Yokai will remove the seal in this location",
            YokaiActionType.Remove => "Remove\n\nThe Yokai will remove the seal in this location. Try not to place any seal here",
            YokaiActionType.ElementalAttack => $"{action.Element} attack\n\nThe Yokai will attack this location with {action.Element}.\n{action.Element.Destroys()} will not offer any protection, but {action.Element.BlockedBy()} will stagger the Yokai.\nAny other element will be removed",
            _ => "ERROR"
        };
        }
    }

    public static async Task Perform (this YokaiAI yokai, YokaiAction action, int i) {
        if (action.IsBlocked) return;

        switch (action.Type) {
            case YokaiActionType.ElementalAttack:
                if (yokai.Incinerate && BattleScene.SealSlots[i] == Element.Fire) {
                    await yokai.AttackOn(i, true);
                    break;
                }
                if (BattleScene.SealSlots[i] == action.Element.Destroys()) {
                    BattleScene.Instance.LogPanel.Log($"The {BattleScene.SealSlots[i].ToSealString()} didn't offer any protection");
                    BattleScene.SealSlots[i] = Element.None;
                    goto case YokaiActionType.Attack;
                }
                if (BattleScene.SealSlots[i] == action.Element.BlockedBy()) {
                    BattleScene.Instance.LogPanel.Log($"Your {BattleScene.SealSlots[i].ToSealString()} staggers the Yokai");
                    yokai.StaggerLevel += 1;
                    break;
                }
                await yokai.AttackOn(i, true);
                await yokai.Remove(i, true);
                break;
            case YokaiActionType.Attack:
                await yokai.AttackOn(i);
                break;
            case YokaiActionType.AttackAndRemove:
                await yokai.AttackOn(i);
                await yokai.Remove(i);
                break;
            case YokaiActionType.Remove:
                await yokai.Remove(i);
                break;
        }
    }

    static async Task Remove (this YokaiAI yokai, int i, bool force = false) {
        if (!force && BattleScene.SealSlots[i] == Element.Water) {
            BattleScene.Instance.LogPanel.Log($"The Yokai tries to remove a [water-seal], but it turns into an [earth-seal]");
            await BattleScene.Instance.PlaceSeal(Element.Earth, i);
        } else if (yokai.Incinerate && BattleScene.SealSlots[i] == Element.Fire) {
            BattleScene.Instance.LogPanel.Log($"The Yokai removes a [fire-seal] and is staggered due to Incineration");
            yokai.StaggerLevel += 1;
            await BattleScene.Instance.RemoveSeal(i);
        } else
            await BattleScene.Instance.RemoveSeal(i);
    }

    static async Task AttackOn (this YokaiAI yokai, int i, bool force = false) {
        if (!force && BattleScene.SealSlots[i] == Element.Metal) {
            BattleScene.Instance.LogPanel.Log($"Your [metal-seal] staggers the Yokai, and turns into an [earth-seal]");
            yokai.StaggerLevel += 1;
            await BattleScene.Instance.PlaceSeal(Element.Earth, i);
            return;
        } else if (yokai.Incinerate && BattleScene.SealSlots[i] == Element.Fire) {
            BattleScene.Instance.LogPanel.Log($"The Yokai attacks a [fire-seal] and is staggered due to Incineration");
            yokai.StaggerLevel += 1;
            return;
        } else if (BattleScene.SealSlots[i] != Element.None) {
            return;
        }
        BattleScene.Instance.LogPanel.Log($"The Yokai attacks you for 1 damage");
        BattleScene.Health -= 1;
        return;
    }

}