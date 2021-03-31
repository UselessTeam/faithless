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
        PlanNextTurn();
    }

    public void PlanNextTurn () {
        ActionPlan = Enumerable.Repeat(YokaiAction.None, BattleScene.SealCount).ToList(); ;
        for (int i = 0 ; i < Yokai.Data().DifficultyValue - ((isStaggered) ? 1 : 0) ; i++) {
            int location = RNG.rng.Next(0, BattleScene.SealCount);
            while (ActionPlan[location] != YokaiAction.None) location = RNG.rng.Next(0, BattleScene.SealCount);
            ActionPlan[location] = YokaiActionExtention.YokaiActionList[i];
        }

        isStaggered = false;
        BattleScene.SealingCircle.DisplayActionPlan(ActionPlan);
        BattleScene.Instance.StartPlayerTurn();
    }

    public bool CheckWinCondition () {
        return true;
    }
}