using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class HandHolder : Control {
    static readonly object handFlowLock = new object();
    public IEnumerable<CardId> Cards => visuals.Select(visual => visual.Card);
    public CardVisual Selected;

    public bool CardInputMode = false;
    public int InputIndex;
    [Signal] public delegate int CardInput ();

    List<CardVisual> visuals = new List<CardVisual>();
    HBoxContainer container;
    [Export] int minWidth;
    Tween tween;
    float split = 0f;
    const int CARDS_WIDTH = 180;

    public override void _Ready () {
        container = GetNode<HBoxContainer>("Container");

        tween = new Tween();
        AddChild(tween);
        Refresh();
    }

    public CardVisual VisualAt (int i) => visuals[i];

    public async Task AddCard (CardId card) {
        CardVisual visualCard;
        lock (handFlowLock) {
            visualCard = CardVisual.Instance();
            visuals.Add(visualCard);
        }
        container.AddChild(visualCard);
        visualCard.Modulate = new Color(1, 1, 1, 0);
        visualCard.ShowCard(card.Data());
        visualCard.MoveFrom(new Vector2(1000, 0));
        await ToSignal(visualCard.MyTween, "tween_completed");
        Refresh();
        visualCard.Connect(nameof(CardVisual.OnClick), this, nameof(SelectCard), visualCard.InArray());
        visualCard.Connect(nameof(CardVisual.FocusEntered), this, nameof(HoverCard));
        visualCard.Connect(nameof(CardVisual.FocusExited), this, nameof(UnHoverCard));
    }

    public void DeselectCard () {
        if (Selected != null) {
            Selected.Pull(0f);
            Selected = null;
            BattleScene.Instance.SealGlow();
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

    public Task DiscardCardVisual (int visualId) {
        if (visualId >= visuals.Count) {
            GD.PrintErr("Trying to delete cardVisual {visualId}, which does not exist");
            return Task.Delay(0);
        }
        return DiscardCardVisual(visuals[visualId]); //Santy  check
    }
    public Task DiscardCardVisual (CardVisual visual) {
        //Sanity check
        if (visual == Selected) {
            DeselectCard();
            BattleScene.Instance.DescribeCard(CardId.None);
        }
        visual.IsDisabled = true;
        visual.Disappear(split);
        lock (handFlowLock) {
            visuals.Remove(visual);
        }
        return DiscardInternal(visual);
    }

    public async Task DiscardInternal (CardVisual visual) {
        await ToSignal(visual.MyTween, "tween_all_completed");
        visual.QueueFree();
        BattleScene.Instance.DisplayDeckAndDiscard();
    }

    public void SelectCard (byte index, CardVisual visual) {
        if (BattleScene.Instance.IsBusy()) {
            if (CardInputMode) {
                InputIndex = index;
                EmitSignal(nameof(CardInput), index);
            }
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

        BattleScene.Instance.SealGlow(visual.Card.Data().Target);
    }

    /*** Display ***/
    private float SplitFor (float cardCount) {
        float width = Math.Max(RectSize.x, minWidth);
        float split = (width - cardCount * CARDS_WIDTH);
        if (split < 0) {
            return split / (cardCount - 1);
        } else {
            return 0;
        }
    }
    public override void _Notification (int notification) {
        Refresh();
    }

    public void Refresh () {
        // Split
        float newSplit = SplitFor(visuals.Count);
        if (split != newSplit) {
            if (IsInstanceValid(container)) {
                tween.InterpolateProperty(container, "custom_constants/separation", split, newSplit, 0.3f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                tween.Start();
            }
            split = newSplit;
        }
        // Scale
        float scale = Math.Min(1f, RectSize.x / minWidth);
        if (IsInstanceValid(container)) {
            container.RectPosition = Vector2.Zero;
            container.RectScale = new Vector2(scale, scale);
        }
    }
}