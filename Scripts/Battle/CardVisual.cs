using System;
using Godot;

public class CardVisual : Control {
    public void ShowCard (CardId card) {
        GetNode<TextureRect>(nameof(TextureRect)).Texture = CardTextures.Instance.GetTexture(card);
        GetNode<TextureRect>(nameof(TextureRect)).Show();
        //TODO (Show kanji and name)
    }
}