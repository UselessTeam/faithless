using System;
using Godot;
using Utils;

public class ShopPanel : ScaleContainer {
    [Export] NodePath leftCardPath;
    [Export] NodePath rightCardPath;
    [Export] NodePath itemPath;
    [Export] NodePath bubblePath;
    [Export] NodePath buyButtonPath;
    CardVisual leftCard;
    CardVisual rightCard;
    TextureRect itemField;
    RichTextLabel bubbleText;
    Button buyButton;
    RichTextLabel buyButtonText;

    // Variables

    CardId left;
    CardId right;
    string item;

    public override void _Ready () {
        leftCard = GetNode<CardVisual>(leftCardPath);
        rightCard = GetNode<CardVisual>(rightCardPath);
        itemField = GetNode<TextureRect>(itemPath);
        bubbleText = GetNode<RichTextLabel>(bubblePath);
        buyButton = GetNode<Button>(buyButtonPath);
        buyButtonText = GetNode<RichTextLabel>(buyButtonPath + "/Text");

        leftCard.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard), 0.InArray());
        rightCard.Connect(nameof(CardVisual.OnClick), this, nameof(OpenCard), 1.InArray());
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
        Load(CardId.BasicMetal, CardId.BasicFire);
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
    public void Load (CardId _left, CardId _right, string _item = "") {
        left = _left;
        right = _right;
        item = _item;
        if (left == CardId.None) {
            leftCard.Hide();
        } else {
            leftCard.ShowCard(left.Data());
            leftCard.Show();
        }
        if (right == CardId.None) {
            rightCard.Hide();
        } else {
            rightCard.ShowCard(right.Data());
            rightCard.Show();
        }
        //TODO: Item
    }

    int selected = -1;

    private Card GetCard (int index) {
        return index switch
        {
            0 => left.Data(),
            1 => right.Data(),
            _ => throw new Exception($"No card at index {index}"),
        };
    }
    public void OpenCard (int _, int index) {
        Card card = GetCard(index);
        selected = index;
        bubbleText.Text = $"Want to buy a {card.Name}?\n\n{card.Description}";
        buyButton.Show();
        // TODO: Check price
        buyButtonText.BbcodeText = $"[center]Buy (TODO: PRICE {BB.Mon})[/center] ";
    }
    public void Buy () {
        Card card = GetCard(selected);
        bubbleText.Text = $"Here is your {card.Name}.\nIt's a pleasure doing business with you.";
        buyButton.Hide();
        GD.Print("TODO: Buy");
    }
}
