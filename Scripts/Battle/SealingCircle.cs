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

    int SlotCount = 0;
    Node2D SealSlotDisplays { get { return GetNode<Node2D>("SealSlotDisplays"); } }
    Node2D ArrowDisplays { get { return GetNode<Node2D>("ArrowDisplays"); } }
    List<DemonAction> actionPlan;
    bool isStaggered = false;


    Vector2 CenterPosition;
    Vector2 CenterToSlot;


    ////////////////////////////////
    ////////// Display
    /////////////
    ///////
    public void InitializeSlots (int slotCount) {
        SlotCount = slotCount;
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

    public void AddSeal () { }
    public void RemoveSeal () { }

    public void DisplaySeals () {
        byte i = 0;
        foreach (var slot in BattleScene.SealSlots) {
            SealSlotDisplays.GetChild<SealSlot>(i).ShowSlot(slot);
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

    public async Task PlayDemonTurn () {
        byte i = 0;
        foreach (var action in actionPlan) {
            switch (action) {
                case DemonAction.Attack:
                case DemonAction.AttackPierce:
                    if (BattleScene.SealSlots[i] == Element.None)
                        BattleScene.Health -= 1;
                    if (BattleScene.SealSlots[i] == Element.Metal) isStaggered = true;
                    break;
                case DemonAction.Remove:
                    BattleScene.SealSlots[i] = Element.None;
                    break;
                case DemonAction.AttackOrRemove:
                    if (BattleScene.SealSlots[i] == Element.Metal) isStaggered = true;
                    if (BattleScene.SealSlots[i] == Element.None)
                        BattleScene.Health -= 1;
                    else
                        BattleScene.SealSlots[i] = Element.None;
                    break;
            }
            i++;
        }
        DisplaySeals();
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
        GetParent<BattleScene>().StartPlayerTurn();
    }


}

public enum DemonAction { None, Attack, Remove, AttackPierce, AttackOrRemove }