using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class CardsManager {
    static readonly object flowLock = new object();
    public List<CardId> Deck;
    public List<CardId> Discard;
    public IEnumerable<CardId> Hand => handHolder.Cards;
    public CardId Selected => (handHolder.Selected == null) ? CardId.None : handHolder.Selected.Card;

    HandHolder handHolder;

    public CardsManager (HandHolder hand) {
        this.handHolder = hand;
        Deck = GameData.Instance.Deck.RandomOrder().ToList();
        Discard = new List<CardId>();
    }

    public CardId HandAt (int i) { return handHolder.VisualAt(i).Card; }

    public async Task<bool> DrawCard () {
        CardId card;
        lock (flowLock) {
            if (Deck.Count == 0) {
                if (Discard.Count == 0) {
                    BattleScene.Instance.LogPanel.Log("There is no more cards to draw");
                    return false;
                }
                Deck = Discard.RandomOrder().ToList();
                Discard = new List<CardId>();
            }
            card = Deck[0];
            Deck.RemoveAt(0);
        }

        await handHolder.AddCard(card);
        return true;
    }
    public Task GetCard (CardId card) => handHolder.AddCard(card);

    public Task DiscardSelected (bool banish) {
        CardVisual visual = handHolder.Selected;
        return DiscardCard(visual, banish);
    }
    public async Task DiscardHand (bool excludeSelected) {
        List<Task> tasks = new List<Task>();
        for (int i = Hand.Count() - 1 ; i >= 0 ; i--) {
            if (excludeSelected && handHolder.VisualAt(i) == handHolder.Selected)
                continue;
            tasks.Add(DiscardCard(i));
        }
        foreach (Task task in tasks) {
            await task;
            BattleScene.Instance.DisplayDeckAndDiscard();
        }

    }
    public Task DiscardCard (int card, bool banish = false) {
        return DiscardCard(handHolder.VisualAt(card), banish);
    }

    public async Task<int> StartCardInput () {
        handHolder.CardInputMode = true;
        await handHolder.ToSignal(handHolder, nameof(HandHolder.CardInput));
        return handHolder.InputIndex;
    }

    public bool IsSelected (int index) {
        return handHolder.VisualAt(index) == handHolder.Selected;
    }

    Task DiscardCard (CardVisual card, bool banish = false) {
        if (!Godot.Object.IsInstanceValid(card) || card.IsDisabled) {
            GD.PrintErr("Trying to delete an invalid cardVisual");
            return Task.Delay(0);
        }
        lock (flowLock) {
            if (!banish) {
                Discard.Add(card.Card);
            }
        }
        return handHolder.DiscardCardVisual(card);
    }

    public async Task<bool> DrawLastDiscard () {
        CardId card;
        lock (flowLock) {
            if (Discard.Count == 0) {
                return false;
            }
            card = Discard[Discard.Count - 1];
            Discard.RemoveAt(Discard.Count - 1);
        }
        await handHolder.AddCard(card);
        return true;
    }

    public void ShuffleDeck () {
        lock (flowLock) {
            Deck = Utils.RNG.RandomOrder(Deck).ToList();
        }
    }

}