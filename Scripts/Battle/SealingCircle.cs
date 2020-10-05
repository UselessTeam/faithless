using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

///////////// Could be part of the battle scenes
/////Handles the display of the Seals and the demon's AI
public class SealingCircle : Node2D {
    [Export(PropertyHint.File)] string sealSlotPath;
    [Export(PropertyHint.File)] string arrowPath;
    [Export] NodePath rayCirclePath;
    public RayCircle RayCircle;

    int SlotCount = 0;
    Node2D SealSlotDisplays { get { return GetNode<Node2D>("SealSlotDisplays"); } }
    Node2D ArrowDisplays { get { return GetNode<Node2D>("ArrowDisplays"); } }
    List<DemonAction> actionPlan;
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
        for (byte i = 0 ; i < SlotCount ; i++) {
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

    async public Task AppearSeal (byte index) {
        var currentSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        currentSeal.ShowSlot(BattleScene.SealSlots[index]);
        currentSeal.Change(BattleScene.SealSlots[index]);
        await ToSignal(currentSeal.MyTween, "tween_completed");
    }
    async public Task DisappearSeal (byte index) {
        var currentSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        currentSeal.Change(Element.None);
        await ToSignal(currentSeal.MyTween, "tween_completed");
        currentSeal.ShowSlot(Element.None);
        currentSeal.Modulate = new Color(1, 1, 1, 1);
    }
    async public Task ReplaceSeal (byte index) {
        await DisappearSeal(index);
        await AppearSeal(index);
    }
    async public Task MoveSeal (byte index, byte indexTo, Element element) { //By default, willmove the element that is at index
        var fromSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        var toSeal = SealSlotDisplays.GetChild<SealSlot>(indexTo);
        fromSeal.ShowSlot(element);
        GD.Print("Before the Tween");
        fromSeal.MoveTo(toSeal.RectPosition - fromSeal.RectPosition);
        GD.Print("Tween Started");
        await ToSignal(fromSeal.MyTween, "tween_completed");
        GD.Print("Tween Finished, arrived at ", fromSeal.MySprite.RectPosition);
        toSeal.ShowSlot(element);
        fromSeal.ShowSlot(Element.None);
    }

    public void DisplaySeals () {
        byte i = 0;
        foreach (var slot in BattleScene.SealSlots) {
            SealSlotDisplays.GetChild<SealSlot>(i).ShowSlot(slot);
            RayCircle.SetSlot(slot, i);
            i++;
        }
    }
    public void DisplayActionPlan () {
        ArrowDisplays.QueueFreeChildren();
        for (byte i = 0 ; i < SlotCount ; i++) {
            if (actionPlan[i] != DemonAction.None) {
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

    public void Hover (DemonAction action) {
        BattleScene.Instance.DescribeAction(action);
    }

    public void UnHover (DemonAction _) {
        if (BattleScene.Instance.Hand.Selected == null) {
            BattleScene.Instance.DescribeCard(CardId.None);
        } else {
            BattleScene.Instance.DescribeCard(BattleScene.Instance.Hand.Selected.Card);
        }
    }

    ////////////////////////////////
    ////////// Demon AI
    /////////////
    ///////

    bool AttackOn (byte i) {
        if (BattleScene.SealSlots[i] == Element.Metal) {
            isStaggered = true; //Cool Staggered effect
            return false;
        } else if (BattleScene.SealSlots[i] != Element.None) {
            return false;
        }
        BattleScene.Health -= 1;
        return true;

    }

    public async Task PlayDemonTurn () {
        byte i = 0;
        foreach (var action in actionPlan) {
            switch (action) {
                case DemonAction.Attack:
                case DemonAction.AttackPierce:
                    AttackOn(i);
                    break;
                case DemonAction.AttackOrRemove:
                    AttackOn(i);
                    await BattleScene.Instance.RemoveSeal(i);
                    break;
                case DemonAction.Remove:
                    await BattleScene.Instance.RemoveSeal(i);
                    break;
            }
            i++;
        }
        DisplaySeals(); // Sanity check
        PlanNextDemonTurn();
    }

    public void PlanNextDemonTurn () {
        actionPlan = Enumerable.Repeat(DemonAction.None, SlotCount).ToList(); ;
        for (int i = 0 ; i < GameData.Instance.Oni.DifficultyValue - ((isStaggered) ? 1 : 0) ; i++) {
            int location = RNG.rng.Next(0, SlotCount);
            while (actionPlan[location] != DemonAction.None) location = RNG.rng.Next(0, SlotCount);
            actionPlan[location] = DemonActionExtention.DemonActionList[i];
        }

        isStaggered = false;
        DisplayActionPlan();
        BattleScene.Instance.StartPlayerTurn();
    }

}

public enum DemonAction { None, Attack, Remove, AttackPierce, AttackOrRemove }

public static class DemonActionExtention {
    public static List<DemonAction> DemonActionList = new List<DemonAction>() {
        DemonAction.Attack, DemonAction.Remove, DemonAction.Remove, DemonAction.AttackOrRemove, DemonAction.Remove, DemonAction.Attack, DemonAction.Remove,
    };

    public static string Description (this DemonAction action) {
        return action
        switch
        {
            DemonAction.Attack => "Attack\n\nThe demon will attack this location and remove 1 health\nYou can defend yourself by placing a Seal of any type",
            DemonAction.AttackOrRemove => "Attack and Remove\n\nThe demon will attack and remove the seal in this location",
            DemonAction.AttackPierce => "Pierce Attack\n\nRemove one health",
            DemonAction.Remove => "Remove\n\nThe demon will remove the seal in this location",
            _ => "ERROR"
        };
    }
}