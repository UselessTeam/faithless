using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

///////////// Could be part of the battle scenes
/////Handles the display of the Seals and the yokai's AI
public class SealingCircle : Node2D {
    [Export(PropertyHint.File)] string sealSlotPath;
    [Export(PropertyHint.File)] string arrowPath;
    [Export] NodePath rayCirclePath;
    public RayCircle RayCircle;

    int SlotCount = 0;
    Node2D SealSlotDisplays { get { return GetNode<Node2D>("SealSlotDisplays"); } }
    Node2D ArrowDisplays { get { return GetNode<Node2D>("ArrowDisplays"); } }
    List<YokaiAction> actionPlan;
    bool isStaggered = false;

    Vector2 CenterPosition;
    Vector2 CenterToSlot;

    public override void _Ready () {
        RayCircle = GetNode<RayCircle>(rayCirclePath);
    }

    ////////////////////////////////
    ////////// Display
    /////////////
    ///////
    public void InitializeSlots (int slotCount) {
        SlotCount = slotCount;
        RayCircle.SetSlotCount(slotCount);
        CenterPosition = GetNode<Position2D>("Center").Position;
        CenterToSlot = GetNode<Position2D>("FirstSealSlot").Position - GetNode<Position2D>("Center").Position;
        for (int i = 0 ; i < SlotCount ; i++) {
            var slot = GD.Load<PackedScene>(sealSlotPath).Instance().GetNode<SealSlot>("./");
            slot.Circle = this;
            slot.RectPosition = CenterPosition + CenterToSlot;
            slot.id = i;
            // slot.Name = i.ToString();
            SealSlotDisplays.AddChild(slot);
            slot.ShowSlot(Element.None);
            CenterToSlot = CenterToSlot.Rotated(2 * Mathf.Pi / SlotCount);
        }
    }

    async public Task AppearSeal (int index) {
        var currentSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        currentSeal.ShowSlot(BattleScene.SealSlots[index]);
        currentSeal.Change(BattleScene.SealSlots[index]);
        await ToSignal(currentSeal.MyTween, "tween_completed");
    }
    async public Task DisappearSeal (int index) {
        var currentSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        currentSeal.Change(Element.None);
        await ToSignal(currentSeal.MyTween, "tween_completed");
        currentSeal.ShowSlot(Element.None);
        currentSeal.Modulate = new Color(1, 1, 1, 1);
    }
    // async public Task ReplaceSeal (int index) {
    //     // await DisappearSeal(index);
    //     await AppearSeal(index);
    // }
    async public Task MoveSeal (int index, int indexTo, Element element) { //By default, willmove the element that is at index
        var fromSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        var toSeal = SealSlotDisplays.GetChild<SealSlot>(indexTo);
        fromSeal.ShowSlot(element);
        fromSeal.MoveTo(toSeal.RectPosition - fromSeal.RectPosition);
        await ToSignal(fromSeal.MyTween, "tween_completed");
        toSeal.ShowSlot(element);
        // fromSeal.ShowSlot(Element.None);
    }

    public void DisplaySeals () {
        int i = 0;
        foreach (var slot in BattleScene.SealSlots) {
            SealSlotDisplays.GetChild<SealSlot>(i).ShowSlot(slot);
            RayCircle.SetSlot(slot, i);
            i++;
        }
    }
    public void DisplayActionPlan () {
        ArrowDisplays.QueueFreeChildren();
        for (int i = 0 ; i < SlotCount ; i++) {
            if (actionPlan[i] != YokaiAction.None) {
                var arrow = GD.Load<PackedScene>(arrowPath).Instance().GetNode<IntentArrow>("./");
                arrow.GlobalPosition = CenterPosition + CenterToSlot / 2;
                arrow.Rotate(2 * Mathf.Pi * i / SlotCount);
                arrow.ShowArrow(actionPlan[i]);
                arrow.Connect(nameof(IntentArrow.FocusEntered), this, nameof(Hover));
                arrow.Connect(nameof(IntentArrow.FocusExited), this, nameof(UnHover));
                ArrowDisplays.AddChild(arrow);
            }
            CenterToSlot = CenterToSlot.Rotated(2 * Mathf.Pi / SlotCount);
        }
    }

    public void Hover (YokaiAction action) {
        BattleScene.Instance.DescribeAction(action);
    }

    public void UnHover (YokaiAction _) {
        if (BattleScene.Hand.Selected == null) {
            BattleScene.Instance.DescribeCard(CardId.None);
        } else {
            BattleScene.Instance.DescribeCard(BattleScene.Hand.Selected.Card);
        }
    }

    ////////////////////////////////
    ////////// Yokai AI
    /////////////
    ///////

    async Task<bool> AttackOn (int i) {
        if (BattleScene.SealSlots[i] == Element.Metal) {
            BattleScene.Instance.LogPanel.Log($"Your [metal-seal] staggers the Yokai, and turns into an [earth-seal]");
            isStaggered = true; //Cool Staggered effect
            await BattleScene.Instance.SwitchSeal(Element.Earth, i);
            return false;
        } else if (BattleScene.SealSlots[i] != Element.None) {
            return false;
        }
        BattleScene.Instance.LogPanel.Log($"The Yokai attacks you for 1 damage");
        BattleScene.Health -= 1;
        return true;
    }

    public async Task PlayYokaiTurn () {
        int i = 0;
        foreach (var action in actionPlan) {
            switch (action) {
                case YokaiAction.Attack:
                case YokaiAction.AttackPierce:
                    await AttackOn(i);
                    break;
                case YokaiAction.AttackOrRemove:
                    await AttackOn(i);
                    goto case YokaiAction.Remove;
                case YokaiAction.Remove:
                    if (BattleScene.SealSlots[i] == Element.Water) {
                        BattleScene.Instance.LogPanel.Log($"The Yokai tries to remove a [water-seal], but it turns into an [earth-seal]");
                        await BattleScene.Instance.SwitchSeal(Element.Earth, i);
                    } else
                        await BattleScene.Instance.RemoveSeal(i);
                    break;
            }
            i++;
        }
        DisplaySeals(); // Sanity check
        PlanNextYokaiTurn();
    }

    public void PlanNextYokaiTurn () {
        actionPlan = Enumerable.Repeat(YokaiAction.None, SlotCount).ToList(); ;
        for (int i = 0 ; i < GameData.Instance.Oni.DifficultyValue - ((isStaggered) ? 1 : 0) ; i++) {
            int location = RNG.rng.Next(0, SlotCount);
            while (actionPlan[location] != YokaiAction.None) location = RNG.rng.Next(0, SlotCount);
            actionPlan[location] = YokaiActionExtention.YokaiActionList[i];
        }

        isStaggered = false;
        DisplayActionPlan();
        BattleScene.Instance.StartPlayerTurn();
    }

}

public enum YokaiAction { None, Attack, Remove, AttackPierce, AttackOrRemove }

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
}