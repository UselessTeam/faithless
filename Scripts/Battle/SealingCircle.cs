using System;
using System.Collections.Generic;
using Godot;

public class SealingCircle : Node2D {
	[Export (PropertyHint.File)] string sealSlotPath;

	List<SealSlot> slotList;

	public void ShowSlots (int slotCount) {
		slotList = new List<SealSlot> ();
		GD.Print (sealSlotPath);
		Vector2 CenterPosition = GetNode<Position2D> ("Center").Position;
		Vector2 CenterToSlot = GetNode<Position2D> ("FirstSealSlot").Position - GetNode<Position2D> ("Center").Position;
		for (byte i = 0; i < slotCount; i++) {
			var slot = GD.Load<PackedScene> (sealSlotPath).Instance ().GetNode<SealSlot> ("./");
			slot.Position = CenterPosition + CenterToSlot;
			slotList.Add (slot);
			AddChild (slot);
			GD.Print ("Printing one poop");
			CenterToSlot = CenterToSlot.Rotated (2 * Mathf.Pi / slotCount);
		}
	}
}
