using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class BattleScene : MarginContainer {
    static BattleScene instance;
    public static BattleScene Instance {
        get {
            if (instance == null) GD.PrintErr("Battle Scene is null");
            return instance;
        }
    }

    /*** Nodes ***/
    [Export] NodePath thoughtPath;
    [Export] NodePath thoughtBubblePath;
    [Export] NodePath endTurnPath;
    [Export] NodePath kiPath;
    [Export] NodePath hpPath;
    [Export] NodePath deckPath;
    [Export] NodePath discardPath;
    [Export] NodePath handholderPath;
    [Export] NodePath sealingCirclePath;
    [Export] NodePath demonsPath;
    SmartText thought;
    Control thoughtBubble;
    Label kiField;
    Label hpField;
    Label deckField;
    Label discardField;
    HandHolder hand;
    public static HandHolder Hand { get => Instance.hand; }
    public SealingCircle SealCircleField;

    /*** Others ***/
    public static List<Element> SealSlots;
    public static int SealCount { get { return SealSlots.Count; } }

    public static IEnumerable<CardId> Deck => Hand.Deck;
    public static IEnumerable<CardId> Discard => Hand.Discard;

    short ki;
    short health;
    public static short Ki {
        get => Instance.ki;
        set {
            Instance.ki = value;
            Instance.kiField.Text = $"{value} / {GameData.Instance.MaxKi}";
        }
    }
    public static short Health {
        get => Instance.health;
        set {
            Instance.health = value;
            Instance.hpField.Text = $"{value} / {GameData.Instance.MaxHealth}";
            if (Health <= 0) {
                LostScene.Loose(Instance.GetTree());
            }
        }
    }

    public enum State { PlayerTurn, CardSelected, SomethingHappening, EnemyTurn }
    State currentState = State.EnemyTurn;

    ///////Battle effects
    public bool NextCardFree = false;
    public short HarvestBonus = 0;

    /////Initialization
    ///

    public override void _Ready () {
        instance = this;
        GameData.Instance.State = GameData.GameState.Battle;

        thought = GetNode<SmartText>(thoughtPath);
        thoughtBubble = GetNode<Control>(thoughtBubblePath);

        kiField = GetNode<Label>(kiPath);
        hpField = GetNode<Label>(hpPath);
        deckField = GetNode<Label>(deckPath);
        discardField = GetNode<Label>(discardPath);

        hand = GetNode<HandHolder>(handholderPath);
        SealCircleField = GetNode<SealingCircle>(sealingCirclePath);

        GetNode<Button>(endTurnPath).Connect("button_down", this, nameof(EndPlayerTurn));

        GetNode<Node2D>(demonsPath).GetNode<AnimatedSprite>(GameData.Instance.Oni.Name).Visible = true;

        SealCircleField.InitializeSlots(GameData.Instance.Oni.SealSlots);
        Health = GameData.Instance.MaxHealth;
        SealSlots = Enumerable.Repeat(Element.None, GameData.Instance.Oni.SealSlots).ToList(); ;

        // GD.Print("~~~~~~~");
        DisplayDeckAndDiscard();
        SealCircleField.PlanNextDemonTurn(); // This function will start the player's turn once it's done
    }

    ///////////////////
    //////  Cards Management
    ////////////////
    ///////

    public static async Task DrawCards (int count) {
        for (int i = 0 ; i < count ; i++) {
            if (!await Hand.DrawCard()) {
                break;
            }
            Instance.DisplayDeckAndDiscard();
        }
    }

    async public void StartPlayerTurn () {
        Ki = GameData.Instance.MaxKi;
        await DrawCards(GameData.Instance.CardsPerTurn);
        await StartTurnEffects();
        currentState = State.PlayerTurn;
    }

    async void EndPlayerTurn () {
        if (currentState != State.PlayerTurn) return;
        currentState = State.EnemyTurn;
        await Hand.DiscardAll();
        await EndTurnEffects();
        await SealCircleField.PlayDemonTurn();
    }

    public bool IsBusy () {
        return currentState == State.SomethingHappening;
    }
    public void DescribeCard (CardId card = CardId.None) {
        if (card == CardId.None) {
            thoughtBubble.Hide();
        } else {
            thoughtBubble.Show();
            thought.BbcodeText = BB.Format(card.Data().Description);
        }
    }
    public void DescribeAction (DemonAction action) {
        thoughtBubble.Show();
        thought.BbcodeText = BB.Format(action.Description());
    }

    //////////////////
    ////////  Display
    ///////////////
    //////////

    // private string DebugCards (IEnumerable<CardId> cards) {
    //     string s = "";
    //     foreach (CardId card in cards) {
    //         if (s != "") {
    //             s += ", ";
    //         }
    //         s += $"<{card.Data().Name}>";
    //     }
    //     return s;
    // }

    public void DisplayDeckAndDiscard () {
        deckField.Text = Deck.Count().ToString();
        discardField.Text = Discard.Count().ToString();
        // GD.Print($"=============\nDiscard {DebugCards(Discard)}\nHand {DebugCards(Hand.Cards)}\nPile {DebugCards(Deck)}");
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    async public void ClickOnSealSlot (int id) {
        if (currentState != State.PlayerTurn || Hand.Selected == null) return;
        currentState = State.SomethingHappening;

        CardVisual visual = Hand.Selected;
        CardData card = visual.Card.Data();
        //Use the card
        if ((Ki >= card.Cost || NextCardFree) &&
            CardData.CheckPlayable(card.Id, id)) { //Check if we can play the card
            Ki -= (NextCardFree) ? (short) 0 : (short) card.Cost;
            NextCardFree = false;
            await card.Use(id);
            // Discard the Card
            if (IsInstanceValid(visual) && !visual.IsDisabled) {
                await Hand.DiscardCard(visual, card.BanishAfterUse);
            }
        }
        currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public async Task RemoveSeal (int location) {
        SealSlots[location] = Element.None;
        await SealCircleField.DisappearSeal(location);
    }

    public async Task SwitchSeal (Element element, int location) {
        bool isEmpty = SealSlots[location] == Element.None;
        SealSlots[location] = element;
        if (isEmpty)
            await SealCircleField.AppearSeal(location);
        else
            await SealCircleField.ReplaceSeal(location);

        if (!SealSlots.Contains(Element.None) && GameData.Instance.Oni.CheckWinCondition()) {
            Win();
        }
    }

    public async Task AddSeal (Element element, int location) {
        var OldElement = SealSlots[location];
        SealSlots[location] = element;
        Task task;

        if (OldElement == Element.None)
            task = SealCircleField.AppearSeal(location);
        else task = SealCircleField.ReplaceSeal(location);

        if (element == Element.Earth) {
            int sealCount = BattleScene.SealSlots.Count;
            int locationBefore = (location + sealCount - 1) % sealCount;
            int locationAfter = (location + 1) % sealCount;

            // Act the switch
            var swapElm = BattleScene.SealSlots[locationBefore];
            BattleScene.SealSlots[locationBefore] = BattleScene.SealSlots[locationAfter];
            BattleScene.SealSlots[locationAfter] = swapElm;

            // Display the Switch
            task = BattleScene.Instance.SealCircleField.MoveSeal(locationBefore, locationAfter, BattleScene.SealSlots[locationAfter]);
            task = BattleScene.Instance.SealCircleField.MoveSeal(locationAfter, locationBefore, BattleScene.SealSlots[locationBefore]);

            await task;
            BattleScene.Instance.SealCircleField.DisplaySeals();
        }

        if (!SealSlots.Contains(Element.None) && GameData.Instance.Oni.CheckWinCondition()) {
            Win();
        }

        await task;

        if (element == Element.Water && OldElement == Element.Fire)
            Health += 1;
        if (element == Element.Fire && OldElement == Element.Wood)
            Ki += 1;
        if (element == Element.Wood && OldElement == Element.Water)
            await DrawCards(1);

        SealCircleField.DisplaySeals(); // Sanity check, just in case
    }

    async public Task StartTurnEffects () {
        List<int> BurnSeals = new List<int>();
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood &&
                 (SealSlots[(i + 1) % SealSlots.Count] == Element.Fire ||
                    SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Fire)) { // If there is a fire after or before
                BurnSeals.Add(i);
            }
        }
        foreach (int i in BurnSeals) {
            Ki += 1;
            await SwitchSeal(Element.Fire, i);
            SealCircleField.DisplaySeals();
        }
    }

    async public Task EndTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Water ||
                    SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Water) { // If there is a water after or before
                                                                                               // TODO: show a cute water effect on the wood
                    await DrawCards((1 + HarvestBonus));
                }
            }
        }
    }

    public const int MaxSeeds = 4;
    int seeds;
    public static int Seeds {
        get => Instance.seeds;
        set {
            Instance.seeds = value;
            //TODO Display new value
            if (value >= MaxSeeds) {
                //TODO Plant a Wooden seal
                Seeds = value - MaxSeeds;
            }
        }
    }

    public async void Win () {
        await SealCircleField.RayCircle.Seal();
        SealedScene.Win(GetTree());
    }

}