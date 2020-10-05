using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class HandHolder : Container {
    public IEnumerable<CardId> Cards => visuals.Select(visual => visual.Card);
    private List<CardVisual> visuals = new List<CardVisual>();
    public CardVisual Selected;
    HBoxContainer container;
    [Export] int minWidth;
    int cards = 4;
    const int CARDS_WIDTH = 180;

    public override void _Ready () {
        container = GetNode<HBoxContainer>("Container");
        Resize();
    }
    public async Task<CardVisual> DrawCard (CardId card) {
        SetCards(visuals.Count);
        var visualCard = CardVisual.Instance();
        container.AddChild(visualCard);
        visualCard.Modulate = new Color(1, 1, 1, 0);
        visualCard.ShowCard(card.Data());
        visualCard.MoveFrom(new Vector2(1000, 0));
        await ToSignal(visualCard.MyTween, "tween_completed");
        visualCard.Connect(nameof(CardVisual.OnClick), this, nameof(SelectCard), visualCard.InArray());
        visualCard.Connect(nameof(CardVisual.FocusEntered), this, nameof(HoverCard));
        visualCard.Connect(nameof(CardVisual.FocusExited), this, nameof(UnHoverCard));
        visuals.Add(visualCard);
        return visualCard;
    }


    public void DeselectCard () {
        if (Selected != null) {
            Selected.Pull(0f);
            Selected = null;
        }
    }

    public void HoverCard (CardId card) {
        BattleScene.Instance.DescribeCard(card);
    }

    public void UnHoverCard (CardId card) {
        if (Selected == null) {
            BattleScene.Instance.DescribeCard(CardId.None);
        } else {
            BattleScene.Instance.DescribeCard(Selected.Card);
        }
    }
    public async Task DiscardCard (CardVisual visual) {
        if (visual == Selected) {
            DeselectCard();
        }
        visual.IsDisabled = true;
        visual.Disappear();
        BattleScene.Discard.Add(visual.Card);
        await ToSignal(visual.MyTween, "tween_all_completed");
        visuals.Remove(visual);
        visual.QueueFree();
        BattleScene.Instance.DisplayDeckAndDiscard();
    }
    public async Task DiscardAll () {
        List<Task> tasks = new List<Task>();
        foreach (CardVisual card in visuals) {
            tasks.Add(DiscardCard(card));
        }
        foreach (Task task in tasks) {
            await task;
        }
    }
    public void SelectCard (byte _, CardVisual visual) {
        if (BattleScene.Instance.IsBusy()) {
            return;
        }

        if (visual == Selected) {
            DeselectCard();
            return;
        }
        // Unselect previous card
        DeselectCard();

        if (visual == null) {
            return;
        }

        Selected = visual;
        Selected.Pull(-50f);

        if (Selected.Card.Data().Cost > BattleScene.Chi) {
            GD.Print("Not enough Chi"); //Might be usefull?
        }
    }

    /*** Display ***/
    public void SetCards (int total) {
        cards = total;
        Resize();
    }
    public override void _Notification (int notification) {
        Resize();
    }

    public void Resize () {
        float horizontal = Math.Min(1f, RectSize.x / minWidth);
        float width = Math.Max(RectSize.x, minWidth);
        float split = (width - cards * CARDS_WIDTH);
        if (split < 0) {
            split /= cards - 1;
        } else {
            split = 0;
        }
        if (IsInstanceValid(container)) {
            container.RectPosition = Vector2.Zero;
            container.Set("custom_constants/separation", split);
        }
    }
}
