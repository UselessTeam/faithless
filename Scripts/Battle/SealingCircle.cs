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
    RayCircle rayCircle;

    int SlotCount = 0;
    Node2D SealSlotDisplays { get { return GetNode<Node2D>("SealSlotDisplays"); } }
    Node2D ArrowDisplays { get { return GetNode<Node2D>("ArrowDisplays"); } }
    List<DemonAction> actionPlan;
    bool isStaggered = false;


    Vector2 CenterPosition;
    Vector2 CenterToSlot;

    public override void _Ready () {
        rayCircle = GetNode<RayCircle>(rayCirclePath);
    }

    ////////////////////////////////
    ////////// Display
    /////////////
    ///////
    public void InitializeSlots (int slotCount) {
        SlotCount = slotCount;
        rayCircle.SetSlotCount(slotCount);
        CenterPosition = GetNode<Position2D>("Center").Position;
        CenterToSlot = GetNode<Position2D>("FirstSealSlot").Position - GetNode<Position2D>("Center").Position;
        for (byte i = 0 ; i < SlotCount ; i++) {
            var slot = GD.Load<PackedScene>(sealSlotPath).Instance().GetNode<SealSlot>("./");
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
        currentSeal.Appear();
        await ToSignal(currentSeal.MyTween, "tween_completed");
    }
    async public Task DisappearSeal (byte index) {
        var currentSeal = SealSlotDisplays.GetChild<SealSlot>(index);
        currentSeal.Disappear();
        await ToSignal(currentSeal.MyTween, "tween_completed");
    }
    async public Task ReplaceSeal (byte index) {
        await DisappearSeal(index);
        await AppearSeal(index);
    }
    async public Task MoveSeal (byte index, byte indexTo) { }

    public void DisplaySeals () {
        byte i = 0;
        foreach (var slot in BattleScene.SealSlots) {
            SealSlotDisplays.GetChild<SealSlot>(i).ShowSlot(slot);
            rayCircle.SetSlot(slot, i);
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
                ArrowDisplays.AddChild(arrow);
            }
            CenterToSlot = CenterToSlot.Rotated(2 * Mathf.Pi / SlotCount);
        }
    }


    ////////////////////////////////
    ////////// Demon AI
    /////////////
    ///////

    bool AttackOn (byte i) {
        if (BattleScene.SealSlots[i] == Element.None) {
            // TODO cool attack effect
            BattleScene.Health -= 1;
            return true;
        }
        if (BattleScene.SealSlots[i] == Element.Metal) isStaggered = true; //Cool Staggered effect
        return false;

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
                    if (!AttackOn(i))
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
        actionPlan[0] = DemonAction.Attack;
        if (!isStaggered) {
            actionPlan[RNG.rng.Next(1, SlotCount)] = DemonAction.Remove;
        }

        isStaggered = false;
        DisplayActionPlan();
        BattleScene.Instance.StartPlayerTurn();
    }


}

public enum DemonAction { None, Attack, Remove, AttackPierce, AttackOrRemove }