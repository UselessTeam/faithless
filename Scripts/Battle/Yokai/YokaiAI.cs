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

    int staggerLevel = 0;
    public int StaggerLevel {
        get { return staggerLevel; }
        set {
            staggerLevel = value;  //Cool Staggered effect}
        }
    }

    //Card effects
    public bool Incinerate = false;

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
        ActionPlan = Enumerable.Repeat(new YokaiAction(YokaiActionType.None), BattleScene.SealCount).ToList();
        int level;
        int actionCount;
        int location;
        YokaiActionType action;
        switch (Yokai) {
            case YokaiId.Hitotsumekozo:
            case YokaiId.Chochinobake:
                // Attacks the bottom seal, and remove some other seals
                level = Yokai switch {
                    YokaiId.Hitotsumekozo => 1,
                    YokaiId.Chochinobake => 2,
                    _ => 0
                };
                actionCount = level switch {
                    0 => 0,
                    1 => 0,
                    _ => 1,
                };
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 4) actionCount += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) actionCount += 1;

                ActionPlan[0] = new YokaiAction(level switch {
                    0 => YokaiActionType.None,
                    1 => YokaiActionType.Attack,
                    _ => YokaiActionType.AttackOrRemove,
                }, (StaggerLevel > 0));
                StaggerLevel -= 1;

                actionCount = Math.Min(actionCount, BattleScene.SealCount - 1); //Prevent infinite loop

                var slots = Enumerable.Range(1, BattleScene.SealCount - 1).ToList();
                for (int i = 0 ; i < actionCount ; i++) {
                    location = slots.Random();
                    while (ActionPlan[location].Type != YokaiActionType.None) location = slots.Random();
                    ActionPlan[location] = new YokaiAction((BattleScene.SealSlots[location] == Element.None) ? YokaiActionType.Attack : YokaiActionType.Remove, (StaggerLevel > 0));
                    StaggerLevel -= 1;
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
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 3) level += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) level += 1;
                actionCount = level switch {
                    0 => 1,
                    1 => 2,
                    2 => (Yokai == YokaiId.Kasaobake) ? 2 : 3,
                    3 => 4,
                    4 => 6,
                    _ => 6,
                };
                int nSuperAttacks = level switch {
                    0 => 1,
                    1 => 1,
                    2 => 2,
                    3 => 2,
                    4 => 3,
                    _ => 4,
                };
                nSuperAttacks -= staggerLevel;

                action = (TurnCount % 2 == 0) ? YokaiActionType.Attack : YokaiActionType.Remove;

                int distanceBetweenActions = BattleScene.SealCount / actionCount;
                location = RNG.rng.Next(0, distanceBetweenActions);

                for (int i = 0 ; i < actionCount ; i++) {
                    bool isBlocked = (StaggerLevel > 0);
                    StaggerLevel--;
                    bool isSupperAttack = (RNG.rng.Next(0, actionCount - i) < nSuperAttacks);
                    if (isSupperAttack) nSuperAttacks--;
                    ActionPlan[location + i * BattleScene.SealCount / actionCount]
                            = new YokaiAction(isSupperAttack ? YokaiActionType.AttackOrRemove : action, isBlocked);
                }
                break;

        }
        staggerLevel = 0;
        Incinerate = false;
        BattleScene.SealingCircle.DisplayActionPlan(ActionPlan);
    }

    public bool CheckWinCondition () {
        return true;
    }
}