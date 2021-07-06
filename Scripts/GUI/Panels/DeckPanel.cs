using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Utils;

public class DeckPanel : MarginContainer {
    [Export] NodePath gridPath;
    [Export] NodePath gridContainerPath;
    [Export] NodePath inspectPath;
    [Export] NodePath banishPath;
    [Export] NodePath pricePath;
    AdjustableGrid gridField;
    SmartText inspectField;
    Button banishField;
    SmartText priceField;
    public override void _Ready () {
        gridField = GetNode<AdjustableGrid>(gridPath);
        inspectField = GetNode<SmartText>(inspectPath);
        banishField = GetNode<Button>(banishPath);
        banishField.Connect("pressed", this, nameof(Banish));
        priceField = GetNode<SmartText>(pricePath);
        ShowDeck();
        Callback.Connect(this, "draw", ShowDeck);
    }

    public void ShowDeck () {
        GD.Print("Draw");
        CloseCard();
        GameData.Instance.Deck = GameData.Instance.Deck.OrderBy(card => (int) card).ToList();
        gridField.QueueFreeChildren();
        int index = 0;
        foreach (CardId id in GameData.Instance.Deck) {
            CardVisual visual = CardVisual.Instance();
            gridField.AddChild(visual);
            // GD.Print($"card {id}");
            visual.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard));
            visual.ShowCard(id.Data());
            index++;
        }
    }

    public void UpdateDeck () {
        gridField.QueueFreeChildren();
        int index = 0;
        foreach (CardId id in GameData.Instance.Deck) {
            CardVisual visual = CardVisual.Instance();
            gridField.AddChild(visual);
            // GD.Print($"card {id}");
            visual.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard));
            visual.ShowCard(id.Data());
            index++;
        }
    }

    public void CloseCard () {
        inspectField.Hide();
        banishField.Disabled = true;
        priceField.BbcodeText = $"[center]Banish[/center]";
    }

    CardData opened;
    const int BANISHMENT_COST = 150;

    public void OpenCard (int index) {
        opened = GameData.Instance.Deck[index].Data();
        inspectField.BbcodeText = BB.Format(opened.Description);
        inspectField.Show();
        banishField.Disabled = (GameData.Instance.Money < BANISHMENT_COST);
        priceField.BbcodeText = $"[center]Banish ({BANISHMENT_COST} {BB.Mon})[/center]";
    }

    public void Banish () {
        GameData.Instance.Money -= BANISHMENT_COST;
        GameData.Instance.Deck.Remove(opened.Id);
        GameData.Instance.DeckChange();
    }
}
