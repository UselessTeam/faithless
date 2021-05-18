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
    [Export] NodePath logPanelPath;
    [Export] NodePath deckPath;
    [Export] NodePath discardPath;
    [Export] NodePath handholderPath;
    [Export] NodePath sealingCirclePath;
    [Export] NodePath yokaiSpritePath;
    public static NodePath yokaiHitBoxPath;
    public YokaiHitBox Yokai;

    SmartText thought;
    Control thoughtBubble;
    Label kiField;
    Label hpField;
    Label deckField;
    Label discardField;
    HandHolder hand;
    public static HandHolder Hand { get => Instance.hand; }
    public static SealingCircle SealingCircle { get; private set; }
    public LogPanel LogPanel { get; private set; }

    ////// Seals
    ///
    public static List<Element> SealSlots;
    public static int SealCount { get { return SealSlots.Count; } }

    ////// Cards
    ///
    public static IEnumerable<CardId> Deck => Hand.Deck;
    public static IEnumerable<CardId> Discard => Hand.Discard;

    ////// Health, Ki and Stats 
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

    public const int MaxSeeds = 4;
    int seeds;
    public static int Seeds {
        get => Instance.seeds;
    }

    ////// Yokai
    ///
    public static YokaiAI YokaiAI;

    /////// Battle State
    ///
    public enum State { PlayerTurn, CardSelected, SomethingHappening, EnemyTurn, SealingYokai }
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
        LogPanel = GetNode<LogPanel>(logPanelPath);
        deckField = GetNode<Label>(deckPath);
        discardField = GetNode<Label>(discardPath);

        hand = GetNode<HandHolder>(handholderPath);
        SealingCircle = GetNode<SealingCircle>(sealingCirclePath);

        Yokai = GetNode<YokaiHitBox>(yokaiHitBoxPath);
        Yokai.Connect(nameof(YokaiHitBox.OnClick), this, nameof(ClickOnTarget));

        GetNode<Button>(endTurnPath).Connect("button_down", this, nameof(EndPlayerTurn));

        var yokai = GameData.Instance.yokai.Data();
        GetNode<Node2D>(yokaiSpritePath).GetNode<AnimatedSprite>(yokai.Name).Visible = true;

        SealingCircle.InitializeSlots(yokai.SealSlots);
        Health = GameData.Instance.MaxHealth;
        SealSlots = Enumerable.Repeat(Element.None, yokai.SealSlots).ToList(); ;

        // GD.Print("~~~~~~~");
        DisplayDeckAndDiscard();
        YokaiAI = new YokaiAI(yokai.Id);
        YokaiAI.PlanNextTurn(); // This function will start the player's turn once it's done
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
        BattleScene.Instance.LogPanel.Separate();
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
        await YokaiAI.PlayTurn();
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
    public void DescribeAction (YokaiAction action) {
        thoughtBubble.Show();
        thought.BbcodeText = BB.Format(action.Description());
    }
    public void DescribeSeal (Element element) {
        thoughtBubble.Show();
        if (element == Element.None) DescribeCard();
        else
            thought.BbcodeText = BB.Format(element.Description());
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
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    async public void ClickOnTarget (int id) {
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
        if (currentState != State.SealingYokai)
            currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public void SealGlow (CardTarget target = null) {
        if (target != null && target.CheckTargetableFunc(-1)) {
            Yokai.StartGlow();
        } else {
            Yokai.StopGlow();
        }
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (target != null && target.CheckTargetableFunc(i)) {
                SealingCircle.GetSeal(i).StartGlow();
            } else {
                SealingCircle.GetSeal(i).StopGlow();
            }
        }
    }

    public async Task RemoveSeal (int location) {
        SealSlots[location] = Element.None;
        await SealingCircle.DisappearSeal(location);
        if (Instance.seeds == MaxSeeds) {//If there was a tree ready to bloom
            await PlaceSeal(Element.Wood, location);
        }
    }

    public async Task SwitchSeal (Element element, int location) {
        bool isEmpty = SealSlots[location] == Element.None;
        SealSlots[location] = element;
        await SealingCircle.AppearSeal(location);

        if (!SealSlots.Contains(Element.None) && YokaiAI.CheckWinCondition()) {
            Win();
        }
    }

    public async Task PlaceSeal (Element element, int location) {
        var OldElement = SealSlots[location];

        List<Task> tasks = new List<Task>();
        tasks.Add(SwitchSeal(element, location));

        if (element == Element.Earth) {
            int sealCount = BattleScene.SealSlots.Count;
            int locationBefore = (location + sealCount - 1) % sealCount;
            int locationAfter = (location + 1) % sealCount;

            // Act the switch
            var swapElm = BattleScene.SealSlots[locationBefore];
            BattleScene.SealSlots[locationBefore] = BattleScene.SealSlots[locationAfter];
            BattleScene.SealSlots[locationAfter] = swapElm;

            // Display the Switch
            if (BattleScene.SealSlots[locationAfter] != Element.None)
                tasks.Add(BattleScene.SealingCircle.MoveSeal(locationBefore, locationAfter, BattleScene.SealSlots[locationAfter]));
            if (BattleScene.SealSlots[locationBefore] != Element.None)
                tasks.Add(BattleScene.SealingCircle.MoveSeal(locationAfter, locationBefore, BattleScene.SealSlots[locationBefore]));
        }
        foreach (Task task in tasks)
            await task;

        SealingCircle.DisplaySeals(); // Sanity check, just in case

        if (element == Element.Fire && OldElement == Element.Wood)
            Ki += 1;
        if (element == Element.Wood && OldElement == Element.Water)
            await DrawCards(1);

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
            LogPanel.Log($"Your [wood-seal] burns, you gain 1 [ki]");
            Ki += 1;
            await SwitchSeal(Element.Fire, i);
            SealingCircle.DisplaySeals();
        }
    }

    async public Task EndTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Water ||
                    SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Water) { // If there is a water after or before
                                                                                               // TODO: show a cute water effect on the wood
                    LogPanel.Log($"Your [wood-seal] [?harvest]harversts[/?], you draw {Utils.SmartText.Concatenate(1 + HarvestBonus, "card")}");
                    await DrawCards((1 + HarvestBonus));
                }
            }
        }
    }

    async public static Task AddSeeds (int count) {
        Instance.seeds += count;
        //TODO Display new value
        if (Instance.seeds >= MaxSeeds) {
            List<int> emptyLocations = new List<int>();
            for (int i = 0 ; i < SealCount ; i++)
                if (SealSlots[i] == Element.None) emptyLocations.Add(i);
            if (emptyLocations.Count > 0) {
                Instance.seeds -= MaxSeeds;
                var makeWoodLoc = emptyLocations[RNG.rng.Next(0, emptyLocations.Count)];
                Instance.LogPanel.Log($"You have gathered {MaxSeeds} seeds. A [wood-seal] appears");
                await Instance.PlaceSeal(Element.Wood, makeWoodLoc);
            } else Instance.seeds = MaxSeeds;
        }
    }

    public async void Win () {
        currentState = State.SealingYokai;
        await SealingCircle.RayCircle.Seal();
        SealingCircle.ZIndex = 0;
        SealedScene.Win(GetTree());
    }

    //Debug
    public override void _Input (InputEvent _event) {
        if (OS.IsDebugBuild() && _event.IsActionPressed("win"))
            Win();
    }

}