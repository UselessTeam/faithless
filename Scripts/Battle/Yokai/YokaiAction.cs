using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public enum YokaiAction {
    None,
    Attack,
    Remove,
    AttackPierce,
    AttackOrRemove
}

public static class YokaiActionExtention {
    public static List<YokaiAction> YokaiActionList = new List<YokaiAction>() {
        YokaiAction.Attack, YokaiAction.Remove, YokaiAction.Remove, YokaiAction.AttackOrRemove, YokaiAction.Remove, YokaiAction.Attack, YokaiAction.Remove,
    };

    public static string Description (this YokaiAction action) {
        return action
        switch {
            YokaiAction.Attack => "Attack\n\nThe Yokai will attack this location and remove 1 health\nYou can defend yourself by placing a Seal of any type",
            YokaiAction.AttackOrRemove => "Attack and Remove\n\nThe Yokai will attack, then remove the seal in this location",
            YokaiAction.AttackPierce => "Pierce Attack\n\nRemove one health",
            YokaiAction.Remove => "Remove\n\nThe Yokai will remove the seal in this location",
            _ => "ERROR"
        };
    }

    public static async Task Perform (this YokaiAI yokai, YokaiAction action, int i) {
        switch (action) {
            case YokaiAction.Attack:
            case YokaiAction.AttackPierce:
                await yokai.AttackOn(i);
                break;
            case YokaiAction.AttackOrRemove:
                await yokai.AttackOn(i);
                goto case YokaiAction.Remove;
            case YokaiAction.Remove:
                if (BattleScene.SealSlots[i] == Element.Water) {
                    BattleScene.Instance.LogPanel.Log($"The Yokai tries to remove a [water-seal], but it turns into an [earth-seal]");
                    await BattleScene.Instance.SwitchSeal(Element.Earth, i);
                } else
                    await BattleScene.Instance.RemoveSeal(i);
                break;
        }
    }

    static async Task<bool> AttackOn (this YokaiAI yokai, int i) {
        if (BattleScene.SealSlots[i] == Element.Metal) {
            BattleScene.Instance.LogPanel.Log($"Your [metal-seal] staggers the Yokai, and turns into an [earth-seal]");
            yokai.StaggerLevel += 1;
            await BattleScene.Instance.SwitchSeal(Element.Earth, i);
            return false;
        } else if (BattleScene.SealSlots[i] != Element.None) {
            return false;
        }
        BattleScene.Instance.LogPanel.Log($"The Yokai attacks you for 1 damage");
        BattleScene.Health -= 1;
        return true;
    }

}