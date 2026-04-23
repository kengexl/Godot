using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node2D
{
    public static BattleManager Instance { get; private set; }

    [ExportGroup("回合")]
    public int CurrentEnergy;
    [Export] public int MaxEnergy = 3;

    [ExportGroup("牌堆")]
    public List<CardData> DrawPile = new();
    public List<Card> HandCards = new();
    public List<CardData> DiscardPile = new();

    [ExportGroup("UI父节点")]
    [Export] public Container HandContainer;

    // ====================== DEBUG 用 ======================
    [Export] public Label DebugLabel;
    private string _lastPlayedCardInfo = "暂无"; // 记录最近打出的卡牌
    // ======================================================

    public override void _Ready()
    {
        Instance = this;
        InitDefaultDeck();
        StartNewTurn();
        UpdateDebug();
    }

    void InitDefaultDeck()
    {
        for (int i = 0; i < 10; i++)
        {
            DrawPile.Add(new CardData()
            {
                CardName = "打击",
                Cost = 1,
                Attack = 6,
                Desc = "造成6点伤害"
            });
        }
        ShuffleDrawPile();
    }

    public void ShuffleDrawPile()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        DrawPile = DrawPile.OrderBy(_ => rng.Randf()).ToList();
    }

    public void StartNewTurn()
    {
        CurrentEnergy = MaxEnergy;
        DrawCard(5);
        UpdateDebug();
    }

    void SpawnCard(CardData data)
    {
        var cardPrefab = GD.Load<PackedScene>("res://Scenes/Card.tscn");

        if (cardPrefab == null)
        {
            GD.PrintErr("错误：找不到卡牌场景！请检查路径是否正确！");
            return;
        }

        Card newCard = cardPrefab.Instantiate<Card>();
        newCard.Data = data;
        HandContainer.AddChild(newCard);
        HandCards.Add(newCard);
    }

    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (DrawPile.Count <= 0)
            {
                DrawPile.AddRange(DiscardPile);
                DiscardPile.Clear();
                ShuffleDrawPile();
                PrintDebug("弃牌堆洗回抽牌堆！");
            }

            var data = DrawPile.First();
            DrawPile.RemoveAt(0);
            SpawnCard(data);
        }
        UpdateDebug();
    }

    public void PlayCard(Card card)
    {
        if (CurrentEnergy < card.Data.Cost)
        {
            PrintDebug("费用不足！");
            return;
        }

        CurrentEnergy -= card.Data.Cost;
        
        // 记录打出的卡牌信息
        _lastPlayedCardInfo = $"{card.Data.CardName} (费用:{card.Data.Cost}, 伤害:{card.Data.Attack})";
        PrintDebug($"✅ 成功打出：{_lastPlayedCardInfo}");

        HandCards.Remove(card);
        DiscardPile.Add(card.Data);
        card.QueueFree();

        UpdateDebug();
    }

    public void EndTurn()
    {
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card.Data);
            card.QueueFree();
        }
        HandCards.Clear();

        PrintDebug("回合结束！");
        EnemyTurn();
        StartNewTurn();
    }

    void EnemyTurn()
    {
        PrintDebug("敌人行动中...");
    }

    // ====================== DEBUG 核心方法 ======================
    public void PrintDebug(string msg)
    {
        GD.Print("[DEBUG] " + msg);
        if (DebugLabel != null) DebugLabel.Text = msg;
    }

    public void UpdateDebug()
    {
        string info = @$"
当前费用：{CurrentEnergy}/{MaxEnergy}
抽牌堆：{DrawPile.Count} 张
手牌：{HandCards.Count} 张
弃牌堆：{DiscardPile.Count} 张
最近打出：{_lastPlayedCardInfo}
";
        GD.Print(info);
        if (DebugLabel != null) DebugLabel.Text = info;
    }
    // ============================================================
}