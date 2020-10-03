using System;
using System.Collections.Generic;
using Godot;

public class SealingCircle : Node2D {
    [Export(PropertyHint.File)] string sealSlotPath;

    List<SealSlot> slotList;

    public void InitializeSlots (int slotCount) {
        slotList = new List<SealSlot>();
        Vector2 CenterPosition = GetNode<Position2D>("Center").Position;
        Vector2 CenterToSlot = GetNode<Position2D>("FirstSealSlot").Position - GetNode<Position2D>("Center").Position;
        for (byte i = 0 ; i < slotCount ; i++) {
            var slot = GD.Load<PackedScene>(sealSlotPath).Instance().GetNode<SealSlot>("./");
            slot.RectPosition = CenterPosition + CenterToSlot;
            slot.id = i;
            slot.Name = i.ToString();
            slotList.Add(slot);
            AddChild(slot);
            CenterToSlot = CenterToSlot.Rotated(2 * Mathf.Pi / slotCount);
        }
    }

    public void UpdateSeals () {
        byte i = 0;
        foreach (var slot in BattleScene.SealSlots) {
            GetNode<SealSlot>(i.ToString()).ShowSlot(slot != Element.None);
            i++;
        }
    }


}
