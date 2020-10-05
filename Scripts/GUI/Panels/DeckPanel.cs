using System;
using System.Collections.Generic;
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
        priceField = GetNode<SmartText>(pricePath);
        GameData.Instance.Connect(nameof(GameData.DeckChanged), this, nameof(ShowDeck));
        ShowDeck();
    }

    public void ShowDeck () {
        CloseCard();
        gridField.QueueFreeChildren();
        int index = 0;
        foreach (CardId id in GameData.Instance.Deck) {
            CardVisual visual = CardVisual.Instance();
            gridField.AddChild(visual);
            GD.Print($"card {id}");
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
            GD.Print($"card {id}");
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

    public void OpenCard (int index) {
        CardData card = GameData.Instance.Deck[index].Data();
        inspectField.Text = card.Description;
        inspectField.Show();
        banishField.Disabled = false; //TODO: Banishment price
        priceField.BbcodeText = $"[center]Banish ({card.Cost * 99} {BB.Mon})[/center]";
    }
}
