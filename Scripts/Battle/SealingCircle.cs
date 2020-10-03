using System;
using System.Collections.Generic;
using Godot;

public class SealingCircle : Node2D {
	[Export] ImageTexture SealSlot;

	List<Sprite> slotList;

	public void ShowSlots (int slotCount) {
		slotList = new List<Sprite> (slotCount);

	}

}
