using System;
using System.Collections.Generic;
using System.Linq;
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
    // List<SealSlot> sealSlotDisplays;
    // List<Node2D> arrowDisplays;
    List<DemonAction> actionPlan;

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

    public void DisplaySeals () {
        byte i = 0;
        GD.Print("PrintingSLots");
        foreach (var slot in BattleScene.SealSlots) {
            SealSlotDisplays.GetChild<SealSlot>(i).ShowSlot(slot);
            i++;
        }
    }
    public void DisplayActionPlan () {
        ArrowDisplays.QueueFreeChildren();
        for (byte i = 0 ; i < SlotCount ; i++) {
            if (actionPlan[i] != DemonAction.None) {
                var arrow = GD.Load<PackedScene>(arrowPath).Instance().GetNode<Node2D>("./");
                arrow.GlobalPosition = CenterPosition + CenterToSlot / 2;
                arrow.Rotate(2 * Mathf.Pi * i / SlotCount);
                ArrowDisplays.AddChild(arrow);
            }
            CenterToSlot = CenterToSlot.Rotated(2 * Mathf.Pi / SlotCount);
        }
    }


    ////////////////////////////////
    ////////// Demon AI
    /////////////
    ///////

    public void PlayDemonTurn () {
        byte i = 0;
        foreach (var action in actionPlan) {
            GD.Print(action.ToString());

            switch (action) {
                case DemonAction.Attack:
                case DemonAction.AttackPierce:
                    if (BattleScene.SealSlots[i] == Element.None)
                        BattleScene.Health -= 1;
                    break;
                case DemonAction.Remove:
                    BattleScene.SealSlots[i] = Element.None;
                    break;
                case DemonAction.AttackOrRemove:
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
        actionPlan[RNG.rng.Next(1, SlotCount)] = DemonAction.Remove;
        DisplayActionPlan();
        GetParent<BattleScene>().StartPlayerTurn();
    }


}

public enum DemonAction { None, Attack, Remove, AttackPierce, AttackOrRemove }