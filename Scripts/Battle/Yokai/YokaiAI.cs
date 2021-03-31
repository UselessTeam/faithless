using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class YokaiAI {
    YokaiId Yokai;

    public YokaiAI (YokaiId yokai) { Yokai = yokai; }

    List<YokaiAction> ActionPlan;
    int TurnCount = 0;

    bool isStaggered = false;
    public bool IsStaggered {
        get { return isStaggered; }
        set {
            isStaggered = value;  //Cool Staggered effect}
        }
    }

    public async Task PlayTurn () {
        int i = 0;
        foreach (var action in ActionPlan) {
            await this.Perform(action, i);
            i++;
        }
        BattleScene.SealingCircle.DisplaySeals(); // Sanity check
        TurnCount++;
        PlanNextTurn();
    }

    public void PlanNextTurn () {
        ActionPlan = Enumerable.Repeat(YokaiAction.None, BattleScene.SealCount).ToList();
        int level;
        int actionCount;
        int location;
        YokaiAction action;
        switch (Yokai) {
            case YokaiId.Hitotsumekozo:
            case YokaiId.Chochinobake:
                // Attacks the bottom seal, and remove some other seals
                level = Yokai switch {
                    YokaiId.Hitotsumekozo => 1,
                    YokaiId.Chochinobake => 2,
                    _ => 0
                };
                if (IsStaggered)
                    level -= 1;
                ActionPlan[0] = level switch {
                    0 => YokaiAction.None,
                    1 => YokaiAction.Attack,
                    _ => YokaiAction.AttackOrRemove,
                };
                actionCount = level switch {
                    0 => 0,
                    1 => 0,
                    _ => 1,
                };
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 4) actionCount += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) actionCount += 1;
                actionCount = Math.Min(actionCount,
                                          CardEffectHelper.NonEmptySealsCount(1, BattleScene.SealCount)); //Prevent infinite loop
                var nonempty = CardEffectHelper.GetNonEmptySeals();
                for (int i = 0 ; i < actionCount ; i++) {
                    location = nonempty.Random();
                    while (ActionPlan[location] != YokaiAction.None) location = nonempty.Random();
                    ActionPlan[location] = YokaiAction.Remove;
                }
                break;

            case YokaiId.Kasaobake:
            case YokaiId.Jorogumo:
                // Alternatively attacks and removes seals
                // Evenly distribute the actions around the yokai
                level = Yokai switch {
                    YokaiId.Kasaobake => 1,
                    YokaiId.Jorogumo => 2,
                    _ => 0
                };
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 4) level += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) level += 1;
                actionCount = level switch {
                    0 => 1,
                    1 => 2,
                    2 => (Yokai == YokaiId.Kasaobake) ? 2 : 3,
                    3 => 4,
                    _ => 6,
                };

                action = (TurnCount % 2 == 0) ? YokaiAction.Attack : YokaiAction.Remove;

                location = RNG.rng.Next(0, BattleScene.SealCount / actionCount);
                for (int i = 0 ; i < actionCount ; i++)
                    ActionPlan[location + i * BattleScene.SealCount / actionCount] = action;
                if (!IsStaggered)
                    ActionPlan[location + RNG.rng.Next(0, actionCount) * BattleScene.SealCount / actionCount] = YokaiAction.AttackOrRemove;

                break;

        }
        isStaggered = false;
        BattleScene.SealingCircle.DisplayActionPlan(ActionPlan);
        BattleScene.Instance.StartPlayerTurn();
    }

    public bool CheckWinCondition () {
        return true;
    }
}