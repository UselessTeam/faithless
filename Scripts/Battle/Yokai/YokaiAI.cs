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
            staggerLevel = value; //Cool Staggered effect}
        }
    }

    int attackLocation = 0;

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
        YokaiData data = Yokai.Data();
        int level = data.Level;
        switch (data.Pattern) {
            case Pattern.Default:
            case Pattern.Vertical:
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 4) level += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) level += 1;

                // Attacks the bottom seal, and remove some other seals
                int sideActionCount =
                    (level < 2) ? 0 : (level < 4) ? 1 : (level < 6) ? 2 : (level < 8) ? 3 : 4;

                if (StaggerLevel > 0) attackLocation = attackLocation.NextLocation();

                ActionPlan[attackLocation] = new YokaiAction(
                    (level < 1) ? YokaiActionType.None : (level < 3) ? YokaiActionType.Attack : YokaiActionType.ElementalAttack,
                    data.Element, (StaggerLevel > 0));
                StaggerLevel -= 1;

                sideActionCount = Math.Min(sideActionCount, BattleScene.SealCount - 2); //Prevent infinite loop

                var slots = Enumerable.Range(1, BattleScene.SealCount / 2 - 1).Concat(Enumerable.Range(BattleScene.SealCount / 2 + 1, BattleScene.SealCount / 2 - 1)).ToList();
                for (int i = 0 ; i < sideActionCount ; i++) {
                    int location = slots.Random();
                    while (ActionPlan[location].Type != YokaiActionType.None) location = slots.Random();
                    ActionPlan[location] = new YokaiAction((BattleScene.SealSlots[location] == Element.None) ? YokaiActionType.Attack : YokaiActionType.Remove, (StaggerLevel > 0));
                    StaggerLevel -= 1;
                }

                ActionPlan[attackLocation.OppositeLocation()] = new YokaiAction(
                    (level < 1) ? YokaiActionType.None : (level < 5) ? YokaiActionType.Attack : YokaiActionType.ElementalAttack,
                    data.Element, (StaggerLevel > 0));

                break;

            case Pattern.Rotary:
                // Alternatively attacks and removes seals
                // Evenly distribute the actions around the yokai
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 4) level += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 3) level += 1;
                if (CardEffectHelper.NonEmptySealsCount() > BattleScene.SealCount / 2) level += 1;

                int rotaryActionCount = level
                switch { 0 => 1, 1 => 2, 2 => 2, 3 => 3, 4 => 3, 5 => 4, 6 => 4, 7 => 6, _ => 6, };
                int nSuperAttacks = level
                switch { 0 => 0, 1 => 0, 2 => 1, 3 => 1, 4 => 2, 5 => 2, 6 => 3, 7 => 3, 8 => 4, 9 => 5, _ => 6 };
                nSuperAttacks -= staggerLevel;

                YokaiActionType action = (TurnCount % 2 == 0) ? YokaiActionType.Attack : YokaiActionType.Remove;

                int distanceBetweenActions = BattleScene.SealCount / rotaryActionCount;

                attackLocation = RNG.Get.Next(0, distanceBetweenActions);

                for (int i = 0 ; i < rotaryActionCount ; i++) {
                    bool isBlocked = (StaggerLevel > 0);
                    StaggerLevel--;
                    bool isSupperAttack = (RNG.Get.Next(0, rotaryActionCount - i) < nSuperAttacks);
                    if (isSupperAttack) nSuperAttacks--;
                    ActionPlan[attackLocation + i * BattleScene.SealCount / rotaryActionCount] = new YokaiAction(isSupperAttack ? YokaiActionType.ElementalAttack : action, data.Element, isBlocked);
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