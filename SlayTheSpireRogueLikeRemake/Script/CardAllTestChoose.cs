using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CardAllTestChoose : Control
{
    [ExportGroup("UI 引用")]
    [Export] public HFlowContainer HFlow_Cards;
    [Export] public Label Label_Debug;
    [Export] public Control UI_Choose;
    [Export] public Control UI_Show;
    [Export] public HFlowContainer HFlow_Show;

    [ExportGroup("资源引用")]
    [Export] public PackedScene CardPrefab;

    [Export] public int MaxDeckCount = 20;

    private List<CardData> _allCards = new();
    private Dictionary<string, int> _selectedDict = new();
    private Dictionary<string, Control> _cardUiDict = new();
    private List<CardData> _finalDeck = new();
    public static List<CardData> GlobalAllCardsCache = new();

    public static List<CardData> GlobalBattleDeck = new();

    public override void _Ready()
    {
        Player.GlobalSavedCurrentHp = -999;
        GD.Print("✅ 选卡场景：已重置玩家血量状态");
        ClearAllData();
        LoadCardsFromCsv();
        GenerateSelectableCards();
        UpdateDebugLabel();

        UI_Choose.Visible = true;
        UI_Show.Visible = false;
    }

    private void LoadCardsFromCsv()
    {
        string path = "res://Data/cards.csv";
        _allCards = CsvReader.ReadCardsFromCsv(path);
        GlobalAllCardsCache = new List<CardData>(_allCards);
    }

    private void GenerateSelectableCards()
    {
        if (CardPrefab == null) return;

        foreach (CardData data in _allCards)
        {
            Control cardIns = CardPrefab.Instantiate<Control>();
            cardIns.Name = data.CardName;

            Card cardScript = cardIns as Card;
            cardScript?.SetCardData(data);
            cardScript?.DisableBattleClick();

            cardIns.GuiInput += e => OnCardInput(e, data.CardName);
            cardIns.MouseFilter = MouseFilterEnum.Stop;

            HFlow_Cards.AddChild(cardIns);
            _cardUiDict.TryAdd(data.CardName, cardIns);
        }
    }

    private void OnCardInput(InputEvent evt, string cardName)
    {
        if (evt is not InputEventMouseButton mb || !mb.Pressed) return;

        int totalNow = _selectedDict.Values.Sum();

        if (mb.ButtonIndex == MouseButton.Left)
        {
            if (totalNow >= MaxDeckCount) return;

            if (_selectedDict.ContainsKey(cardName))
                _selectedDict[cardName]++;
            else
                _selectedDict[cardName] = 1;
        }

        if (mb.ButtonIndex == MouseButton.Right)
        {
            if (!_selectedDict.ContainsKey(cardName)) return;

            _selectedDict[cardName]--;
            if (_selectedDict[cardName] <= 0)
                _selectedDict.Remove(cardName);
        }

        UpdateDebugLabel();
    }

    private void UpdateDebugLabel()
    {
        int total = _selectedDict.Values.Sum();
        Label_Debug.Text = $"已选择 {total}/{MaxDeckCount} 张\n";
        foreach (var kv in _selectedDict)
            Label_Debug.Text += $"{kv.Key} x{kv.Value}\n";
    }

    public void OnButtonShowPressed()
    {
        if (_selectedDict.Count == 0)
            return;

        GenerateFinalDeck();
        ShuffleDeck();
        ClearShowUI();
        GenShowDeckUI();

        GlobalBattleDeck.Clear();
        GlobalBattleDeck.AddRange(_finalDeck);

        UI_Choose.Visible = false;
        UI_Show.Visible = true;
    }

    public void OnButtonBackPressed()
    {
        ClearShowUI();
        UI_Show.Visible = false;
        UI_Choose.Visible = true;
    }

    public void OnButtonResetPressed()
    {
        ClearAllData();
        UpdateDebugLabel();
    }

    public void OnStartBattlePressed()
    {
        if (GlobalBattleDeck.Count == 0)
            return;

        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }

    #region 内部工具方法
    private void GenerateFinalDeck()
    {
        _finalDeck.Clear();
        foreach (var kv in _selectedDict)
        {
            var card = _allCards.FirstOrDefault(c => c.CardName == kv.Key);
            if (card == null) continue;
            for (int i = 0; i < kv.Value; i++)
                _finalDeck.Add(card);
        }
    }

    private void ShuffleDeck()
    {
        var rnd = new System.Random();
        _finalDeck = _finalDeck.OrderBy(_ => rnd.Next()).ToList();
    }

    private void ClearShowUI()
    {
        foreach (var child in HFlow_Show.GetChildren())
            child.QueueFree();
    }

    private void GenShowDeckUI()
    {
        foreach (var data in _finalDeck)
        {
            var cardIns = CardPrefab.Instantiate<Control>();
            var cardScript = cardIns as Card;
            cardScript?.SetCardData(data);
            cardScript?.DisableBattleClick();
            cardIns.MouseFilter = MouseFilterEnum.Ignore;
            HFlow_Show.AddChild(cardIns);
        }
    }

    private void ClearAllData()
    {
        _selectedDict.Clear();
        _finalDeck.Clear();
       
    }
    #endregion
}