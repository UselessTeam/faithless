using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Utils;

public class BattleScene : CanvasLayer {
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
    [Export] NodePath seedsPath;
    [Export] NodePath logPanelPath;
    [Export] NodePath deckPath;
    [Export] NodePath discardPath;
    [Export] NodePath discardDisplayPath;
    [Export] NodePath handholderPath;
    [Export] NodePath sealingCirclePath;
    [Export] NodePath yokaiSpritePath;
    public static NodePath yokaiHitBoxPath;
    public YokaiHitBox Yokai;

    SmartText thought;
    Control thoughtBubble;
    Label kiField;
    Label hpField;
    SeedIcon seedsFiled;
    Label deckField;
    Label discardField;
    Discard discardDisplay;
    CardsManager cardManager;
    public static CardsManager Cards { get => Instance.cardManager; }
    public static SealingCircle SealingCircle { get; private set; }
    public LogPanel LogPanel { get; private set; }

    ////// Seals
    ///
    public static List<Element> SealSlots;
    public static int SealCount { get { return SealSlots.Count; } }

    ////// Cards
    ///
    public static IEnumerable<CardId> Deck => Cards.Deck;
    public static IEnumerable<CardId> Discard => Cards.Discard;

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
                SealingCircle.ZIndex = 0;
                LostScene.Lose(Instance);
            }
        }
    }

    public const int MaxSeeds = 4;
    int seeds;
    public static int Seeds {
        get => Instance.seeds;
        set {
            Instance.seeds = value;
            Instance.seedsFiled.UpdateValue(value);
        }
    }

    ////// Yokai
    ///
    public static YokaiAI YokaiAI;

    /////// Battle State
    ///
    public enum State { PlayerTurn, CardSelected, SomethingHappening, EnemyTurn, SealingYokai }
    State currentState = State.EnemyTurn;

    ///////Card effects
    public Dictionary<Element, bool> NextCardFree = new List<Element>() { Element.Fire, Element.Water, Element.Wood, Element.Earth, Element.Metal, Element.None }
        .ToDictionary(element => element, element => false);
    public short HarvestBonus = 0;
    public bool RetainHand = false;

    /////Initialization
    ///

    public override void _Ready () {
        instance = this;
        GameData.Instance.State = GameData.GameState.Battle;
        SFXHandler.Instance.Change(GameData.GameState.Battle);

        thought = GetNode<SmartText>(thoughtPath);
        thoughtBubble = GetNode<Control>(thoughtBubblePath);

        kiField = GetNode<Label>(kiPath);
        hpField = GetNode<Label>(hpPath);
        seedsFiled = GetNode<SeedIcon>(seedsPath);
        LogPanel = GetNode<LogPanel>(logPanelPath);
        deckField = GetNode<Label>(deckPath);
        discardField = GetNode<Label>(discardPath);
        discardDisplay = GetNode<Discard>(discardDisplayPath);

        cardManager = new CardsManager(GetNode<HandHolder>(handholderPath));
        SealingCircle = GetNode<SealingCircle>(sealingCirclePath);

        Yokai = GetNode<YokaiHitBox>(yokaiHitBoxPath);
        Yokai.Connect(nameof(YokaiHitBox.OnClick), this, nameof(ClickOnTarget));

        GetNode<Button>(endTurnPath).Connect("button_down", this, nameof(EndPlayerTurn));

        var yokai = GameData.Instance.yokai.Data();
        GetNode<Node2D>(yokaiSpritePath).GetNode<AnimatedSprite>(yokai.Name).Visible = true;

        SealingCircle.InitializeSlots(yokai.SealSlots);
        Health = GameData.Instance.MaxHealth;
        SealSlots = Enumerable.Repeat(Element.None, yokai.SealSlots).ToList(); ;

        DisplayDeckAndDiscard();
        YokaiAI = new YokaiAI(yokai.Id);
        YokaiAI.PlanNextTurn();
        BattleScene.Instance.StartPlayerTurn();
    }

    ///////////////////
    //////  Cards Management
    ////////////////
    ///////

    public static async Task DrawCards (int count) {
        for (int i = 0 ; i < count ; i++) {
            if (!await Cards.DrawCard()) {
                break;
            }
            Instance.DisplayDeckAndDiscard();
        }
    }

    async public void StartPlayerTurn () {
        BattleScene.Instance.LogPanel.Separate();
        Ki = GameData.Instance.MaxKi;
        await DrawCards(GameData.Instance.CardsPerTurn);
        await TriggerFireSpread();
        currentState = State.PlayerTurn;
    }

    async void EndPlayerTurn () {
        if (currentState != State.PlayerTurn) return;
        currentState = State.EnemyTurn;
        if (!RetainHand)
            await Cards.DiscardHand(false);
        else
            RetainHand = false;
        await TriggerHarvest();
        await YokaiAI.PlayTurn();
        StartPlayerTurn();
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
        discardDisplay.UpdateDiscard();
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    async public void ClickOnTarget (int id) {
        if (currentState != State.PlayerTurn || Cards.Selected == CardId.None) return;
        currentState = State.SomethingHappening;

        CardData card = cardManager.Selected.Data();
        //Use the card
        if (!(Ki >= card.Cost || NextCardFree[card.Element] || NextCardFree[Element.None]))
            LogPanel.Log("You don't have enough [ki] to play this talisman");
        else if (!CardData.CheckPlayable(card.Id, id))
            LogPanel.Log("You cannot play this talisman here");
        else { //Check if we can play the card
            if (NextCardFree[card.Element])
                NextCardFree[card.Element] = false;
            else if (NextCardFree[Element.None])
                NextCardFree[Element.None] = false;
            else
                Ki -= (NextCardFree[card.Element]) ? (short) 0 : (short) card.Cost;
            SFXHandler.PlaySFX(card.SFX);
            await card.Use(id);
            await Cards.DiscardSelected(card.BanishAfterUse);
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
        if (Seeds == MaxSeeds) { //If there was a tree ready to bloom
            await PlaceSeal(Element.Wood, location);
        }
    }

    public async Task PlaceSeal (Element element, int location) {
        bool isEmpty = SealSlots[location] == Element.None;
        SealSlots[location] = element;
        await SealingCircle.AppearSeal(location);

        if (!SealSlots.Contains(Element.None) && YokaiAI.CheckWinCondition()) {
            Win();
        }

        // if (element == Element.Fire && OldElement == Element.Wood)
        //     Ki += 1;
        // if (element == Element.Wood && OldElement == Element.Water)
        //     await DrawCards(1);
    }

    public async Task SwapSeals (int location1, int location2) {
        List<Task> tasks = new List<Task>();
        // Act the switch
        var swapElm = BattleScene.SealSlots[location1];
        BattleScene.SealSlots[location1] = BattleScene.SealSlots[location2];
        BattleScene.SealSlots[location2] = swapElm;

        // Display the Switch
        if (BattleScene.SealSlots[location2] != Element.None)
            tasks.Add(BattleScene.SealingCircle.MoveSeal(location1, location2, BattleScene.SealSlots[location2]));
        if (BattleScene.SealSlots[location1] != Element.None)
            tasks.Add(BattleScene.SealingCircle.MoveSeal(location2, location1, BattleScene.SealSlots[location1]));
        foreach (Task task in tasks)
            await task;
        SealingCircle.DisplaySeals();
    }

    async public Task TriggerFireSpread () {
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
            await PlaceSeal(Element.Fire, i);
            SealingCircle.DisplaySeals();
        }
    }

    async public Task TriggerHarvest () {
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
        Seeds += count;
        //TODO Display new value
        while (Seeds >= MaxSeeds) {
            List<int> emptyLocations = new List<int>();
            for (int i = 0 ; i < SealCount ; i++)
                if (SealSlots[i] == Element.None) emptyLocations.Add(i);
            if (emptyLocations.Count > 0) {
                Seeds -= MaxSeeds;
                var makeWoodLoc = emptyLocations[RNG.rng.Next(0, emptyLocations.Count)];
                Instance.LogPanel.Log($"You have gathered {MaxSeeds} seeds. A [wood-seal] appears");
                SFXHandler.PlaySFX(CardId.BasicWood.Data().SFX);
                await Instance.PlaceSeal(Element.Wood, makeWoodLoc);
            } else {
                Seeds = MaxSeeds;
            }
        }
    }

    bool WinCalled = false;
    public async void Win () {
        if (WinCalled || currentState == State.SealingYokai) {
            if (OS.IsDebugBuild()) GD.Print("Win already called");
            return;
        }
        WinCalled = true;
        currentState = State.SealingYokai;
        await SealingCircle.RayCircle.Seal();
        SealingCircle.ZIndex = 0;
        SealedScene.Win(this);
    }

    //Debug
    public override void _Input (InputEvent _event) {
        if (OS.IsDebugBuild()) {
            if (_event.IsActionPressed("debug_win"))
                Win();
            if (_event.IsActionPressed("debug_draw"))
                DrawCards(1);
            if (_event.IsActionPressed("debug_ki"))
                Ki += 5;
        }
    }
}