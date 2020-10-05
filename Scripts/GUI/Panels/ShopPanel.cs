using System;
using System.Collections.Generic;
using Godot;
using Utils;

public class ShopPanel : ScaleContainer {
    [Export] NodePath leftCardPath;
    [Export] NodePath middleCardPath;
    [Export] NodePath rightCardPath;
    [Export] NodePath foodPath;
    [Export] NodePath bubblePath;
    [Export] NodePath buyButtonPath;
    CardVisual leftCard;
    CardVisual middleCard;
    CardVisual rightCard;
    FoodVisual foodField;
    RichTextLabel bubbleText;
    Button buyButton;
    RichTextLabel buyButtonText;

    // Variables

    CardId left;
    CardId middle;
    CardId right;
    Food food;

    public override void _Ready () {
        leftCard = GetNode<CardVisual>(leftCardPath);
        middleCard = GetNode<CardVisual>(middleCardPath);
        rightCard = GetNode<CardVisual>(rightCardPath);
        foodField = GetNode<FoodVisual>(foodPath);
        bubbleText = GetNode<RichTextLabel>(bubblePath);
        buyButton = GetNode<Button>(buyButtonPath);
        buyButtonText = GetNode<RichTextLabel>(buyButtonPath + "/Text");

        leftCard.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard), 0.InArray());
        middleCard.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard), 1.InArray());
        rightCard.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard), 2.InArray());
        foodField.Connect(nameof(FoodVisual.OnClick), this, nameof(OpenFood));
        buyButton.Connect("pressed", this, nameof(Buy));

        Init();
    }

    private readonly string[] WELCOME_MESSAGES = new string[] {
        "Hello there! Want to buy something?",
        "Cheap seals for sale!",
        "Hello! Want to buy some nice seals?",
    };
    public void Init () {
        bubbleText.Text = WELCOME_MESSAGES.Random();
        buyButton.Hide();
        List<CardId> all = new List<CardId>(CardData.All);
        Load(all.PopRandom(), all.PopRandom(), all.PopRandom(), GameData.Instance.LeftInShop.Count == 0 ? null : GameData.Instance.LeftInShop.Random());
    }

    private readonly string[] AGAIN_MESSAGES = new string[] {
        "Hello again!",
        "Oh! It's you again!",
        "Back for more?",
        "You missed me?",
    };
    public void Refresh () {
        bubbleText.Text = AGAIN_MESSAGES.Random();
        buyButton.Hide();
    }
    public void Load (CardId left, CardId middle, CardId right, Food food) {
        SetCard(left, 0);
        SetCard(middle, 1);
        SetCard(right, 2);
        SetFood(food);
    }

    int selected = -2;

    private CardId GetCard (int index) {
        return index switch
        {
            0 => left,
            1 => middle,
            2 => right,
            _ => throw new Exception($"No card at index {index}"),
        };
    }
    public void SetCard (CardId card, int index) {
        switch (index) {
            case 0:
                left = card;
                if (left == CardId.None) {
                    leftCard.Hide();
                } else {
                    leftCard.ShowCard(left.Data());
                    leftCard.Show();
                }
                return;
            case 1:
                middle = card;
                if (middle == CardId.None) {
                    middleCard.Hide();
                } else {
                    middleCard.ShowCard(middle.Data());
                    middleCard.Show();
                }
                return;
            case 2:
                right = card;
                if (right == CardId.None) {
                    rightCard.Hide();
                } else {
                    rightCard.ShowCard(right.Data());
                    rightCard.Show();
                }
                return;
            default:
                throw new Exception($"No card at index {index}");
        };
    }

    public void SetFood (Food food) {
        this.food = food;
        foodField.SetFood(food);
    }

    public void OpenCard (int _, int index) {
        CardData card = GetCard(index).Data();
        selected = index;
        bubbleText.Text = $"Want to buy a {card.Name}?\n\n{card.Description}";
        buyButton.Show();
        buyButtonText.BbcodeText = $"[center]Buy ({card.MonPrice} {BB.Mon})[/center] ";
        buyButton.Disabled = (card.MonPrice > GameData.Instance.Money);
    }

    public void OpenFood () {
        selected = -1;
        bubbleText.Text = $"Want to buy a {food.Name}?\n\n{food.Description}";
        buyButton.Show();
        buyButtonText.BbcodeText = $"[center]Buy ({food.Price} {BB.Mon})[/center] ";
        buyButton.Disabled = (food.Price > GameData.Instance.Money);
    }
    public void Buy () {
        if (selected == -1) {
            bubbleText.Text = $"Here is your {food.Name}.\nIt's a pleasure doing business with you.";
            buyButton.Hide();
            // Buy food
            food.Effect();
            GameData.Instance.LeftInShop.Remove(food);
            SetFood(null);
        } else {
            CardData card = GetCard(selected).Data();
            bubbleText.Text = $"Here is your {card.Name}.\nIt's a pleasure doing business with you.";
            buyButton.Hide();
            // Buy card
            GameData.Instance.Deck.Add(card.Id);
            GameData.Instance.DeckChange();
            GameData.Instance.Money -= card.MonPrice;
            SetCard(CardId.None, selected);
        }
    }
}
