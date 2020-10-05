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
    [Export] NodePath chiPath;
    [Export] NodePath hpPath;
    [Export] NodePath deckPath;
    [Export] NodePath discardPath;
    [Export] NodePath handholderPath;
    [Export] NodePath sealingCirclePath;
    SmartText thought;
    Control thoughtBubble;
    Label chiField;
    Label hpField;
    Label deckField;
    Label discardField;
    public HandHolder Hand;
    public SealingCircle SealCircleField;


    /*** Others ***/
    public static List<CardId> Deck;
    public static List<CardId> Discard = new List<CardId>();

    public static List<Element> SealSlots;

    const byte MAX_CHI = 5;
    const byte MAX_HEALTH = 3;
    const byte CARDS_PER_TURN = 4;

    short chi;
    short health;
    public static short Chi { get { return Instance.chi; } set { Instance.chi = value; Instance.chiField.Text = value.ToString(); } }
    public static short Health {
        get { return Instance.health; }
        set {
            Instance.health = value; Instance.hpField.Text = value.ToString();
            if (Health <= 0) {
                LostScene.Loose(Instance.GetTree());
            }
        }
    }

    public enum State { PlayerTurn, CardSelected, SomethingHappening, EnemyTurn }
    State currentState = State.EnemyTurn;

    ///////Battle effects
    static public bool NextCardFree = false;
    static public short HarvestBonus = 0;

    /////Initialization
    ///

    public override void _Ready () {
        instance = this;
        GameData.Instance.State = GameData.GameState.Battle;
        Deck = new List<CardId>(GameData.Instance.Deck);

        thought = GetNode<SmartText>(thoughtPath);
        thoughtBubble = GetNode<Control>(thoughtBubblePath);

        chiField = GetNode<Label>(chiPath);
        hpField = GetNode<Label>(hpPath);
        deckField = GetNode<Label>(deckPath);
        discardField = GetNode<Label>(discardPath);

        Hand = GetNode<HandHolder>(handholderPath);
        SealCircleField = GetNode<SealingCircle>(sealingCirclePath);

        GetNode<Button>(endTurnPath).Connect("button_down", this, nameof(EndPlayerTurn));

        SealCircleField.InitializeSlots(GameData.Instance.Oni.SealSlots);
        Health = MAX_HEALTH;
        SealSlots = Enumerable.Repeat(Element.None, GameData.Instance.Oni.SealSlots).ToList(); ;

        Instance.ShuffleDeck();

        SealCircleField.PlanNextDemonTurn(); // This function will start the player's turn once it's done
    }

    ///////////////////
    //////  Cards Management
    ////////////////
    ///////

    public static async Task DrawCards (byte count) {
        for (byte i = 0 ; i < count ; i++) {
            if (Deck.Count == 0) {
                Deck = Discard;
                Discard = new List<CardId>();
                Instance.ShuffleDeck();
            }
            if (Deck.Count == 0) break;
            var addCard = Deck[0];
            CardVisual card = await Instance.Hand.DrawCard(addCard);
            Deck.RemoveAt(0);
        }
    }

    void ShuffleDeck () {
        Deck = Utils.RNG.RandomOrder(Deck).ToList();
    }

    async public void StartPlayerTurn () {
        Chi = MAX_CHI;
        await DrawCards(CARDS_PER_TURN);
        await StartTurnEffects();
        currentState = State.PlayerTurn;
    }

    async void EndPlayerTurn () {
        if (currentState != State.PlayerTurn) return;
        Task task = Hand.DiscardAll();
        currentState = State.EnemyTurn;
        await EndTurnEffects();
        await task;
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

    //////////////////
    ////////  Display
    ///////////////
    //////////


    public void DisplayDeckAndDiscard () {
        deckField.Text = Deck.Count.ToString();
        discardField.Text = Discard.Count.ToString();
    }

    //////////////////////
    ////////  Input Management
    ////////////////
    //////

    async public void ClickOnSealSlot (byte id) {
        if (currentState != State.PlayerTurn || Hand.Selected == null) return;
        currentState = State.SomethingHappening;

        CardVisual visual = Hand.Selected;
        CardData card = visual.Card.Data();
        //Use the card
        if ((Chi >= card.Cost || NextCardFree)
        && CardData.CheckPlayable(card.Id, SealSlots[id])) { //Check if we can play the card
            Chi -= (NextCardFree) ? (short) 0 : (short) card.Cost;
            await card.Use(id);
            NextCardFree = false;
            // Discard the Card
            await Hand.DiscardCard(visual);
        }
        currentState = State.PlayerTurn;
    }

    ///////////////////
    //////  Battle Effects
    ////////////////
    ///////

    public async Task RemoveSeal (byte location) {
        SealSlots[location] = Element.None;
        await SealCircleField.DisappearSeal(location);
    }

    public async Task SwitchSeal (Element element, byte location) {
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

    public async Task AddSeal (Element element, byte location) {
        var OldElement = SealSlots[location];
        SealSlots[location] = element;
        Task task;
        GD.Print("Adding seel");
        if (element == Element.Earth && SealSlots.Contains(Element.None)) { //Earth related movement
            int moveLocation = location;
            var moveElement = OldElement;
            if (moveElement == Element.None) {
                OldElement = Element.Earth;
                moveLocation = (moveLocation + 1) % SealSlots.Count;
                moveElement = SealSlots[moveLocation];
                if (!SealSlots.Contains(Element.None))
                    moveElement = Element.None;
                else
                    SealSlots[moveLocation] = Element.None;
            }
            Task taskMove = null;
            while (moveElement != Element.None) {
                var newMoveLocation = (moveLocation + 1) % SealSlots.Count;
                taskMove = SealCircleField.MoveSeal((byte) moveLocation, (byte) newMoveLocation, moveElement);
                moveLocation = newMoveLocation;
                var tempSwap = SealSlots[moveLocation];
                SealSlots[moveLocation] = moveElement;
                moveElement = tempSwap;
            }
            if (taskMove != null) await taskMove;
        }

        if (OldElement == Element.None)
            task = SealCircleField.AppearSeal(location);
        else task = SealCircleField.ReplaceSeal(location);

        if (!SealSlots.Contains(Element.None) && GameData.Instance.Oni.CheckWinCondition()) {
            Win();
        }

        await task;

        if (element == Element.Water && OldElement == Element.Fire)
            Health += 1;
        if (element == Element.Fire && OldElement == Element.Wood)
            Chi += 1;
        if (element == Element.Wood && OldElement == Element.Water)
            await DrawCards(1);

        SealCircleField.DisplaySeals(); // Sanity check, just in case
    }

    async public Task StartTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Fire
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Fire) {// If there is a fire after or before
                    Chi += 1;

                    await SwitchSeal(Element.Fire, (byte) i);
                    SealCircleField.DisplaySeals(); //Sanity check
                }
            }
        }
    }
    async public Task EndTurnEffects () {
        for (int i = 0 ; i < SealSlots.Count ; i++) {
            if (SealSlots[i] == Element.Wood) {
                if (SealSlots[(i + 1) % SealSlots.Count] == Element.Water
                || SealSlots[(i + SealSlots.Count - 1) % SealSlots.Count] == Element.Water) {// If there is a water after or before
                                                                                             // TODO: show a cute water effect on the wood
                    await DrawCards((byte) (1 + HarvestBonus));
                }
            }
        }
    }

    public async void Win () {
        await SealCircleField.RayCircle.Seal();
        SealedScene.Win(GetTree());
        // GetTree().Paused = true;
    }

    //Debug
    public override void _Input (InputEvent _event) {
        if (_event.IsActionPressed("win"))
            Win();
    }
}