using System;
using System.Collections.Generic;
using Godot;

public enum CardId {
	BasicFire,
	BasicWater,
	BasicWood,
	BasicEarth,
	BasicMetal
}

public class Card : Resource {
	// [Export] readonly ushort Id = ushort.MaxValue;
	[Export] readonly byte Cost = byte.MaxValue;
	[Export] readonly string Description = "This is an empty card";
	[Export] readonly Action Use = () => { GD.Print ("This card does nothing"); };

	public Card (byte cost, string description, Action use) { this.Cost = cost; this.Description = description; this.Use = use; }

	public static Dictionary<CardId, Card> List =
		new Dictionary<CardId, Card> {
			{
			CardId.BasicFire,
			new Card (2, "Place one Fire Seal",
			() => { /*Place one fire seal*/ })
			},
			{
			CardId.BasicWater,
			new Card (2, "Place one Water Seal",
			() => { /*Place one Water seal*/ })
			},
			{
			CardId.BasicWood,
			new Card (2, "Place one Wood Seal",
			() => { /*Place one Wood seal*/ })
			},
			{
			CardId.BasicEarth,
			new Card (2, "Place one Earth Seal",
			() => { /*Place one Earth seal*/ })
			},
			{
			CardId.BasicMetal,
			new Card (2, "Place one Metal Seal",
			() => { /*Place one Metal seal*/ })
			},
		};
}
