using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Control = Godot.Control;

// 战斗管理器：全局唯一单例，负责整个战斗流程
public partial class BattleManager : Node2D
{
    // 单例：让任何脚本都能通过 Instance 访问这里
    public static BattleManager Instance;

    // 编辑器拖拽：玩家角色脚本
    [Export] public Player Player;
    
    // 编辑器拖拽：敌人角色脚本
    [Export] public EnemyBase Enemy;
    
    // 编辑器拖拽：手牌容器（放卡牌的父节点）
    [Export] public Container HandContainer;
    
    // 编辑器拖拽：卡牌预制体
    [Export] public PackedScene CardPrefab;

    // 抽牌堆：存放没抽的卡牌
    public List<CardData> DrawPile = new();
    
    // 手牌：当前拿在手里的卡牌
    public List<Card> HandCards = new();
    
    // 弃牌堆：打出过的卡牌
    public List<CardData> DiscardPile = new();

    // 当前可用费用
    public int CurrentEnergy;
    
    // 每回合最大费用（可在编辑器修改）
    [Export] public int MaxEnergy = 3;
    
    // 每回合抽卡数量
    [Export] public int DrawCardCountPerTurn = 3;

    // 战斗是否结束
    public bool IsBattleEnd;

    // 场景初始化
    public override void _Ready()
    {
        // 单例防重复
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;

        // 校验关键引用
        if (Player == null || Enemy == null || HandContainer == null || CardPrefab == null)
        {
            GD.PrintErr("BattleManager：关键节点未配置！请检查编辑器拖拽赋值");
            return;
        }

        LoadCardsFromCSV();  // 从CSV读取所有卡牌
        Shuffle();           // 洗牌
        StartPlayerTurn();   // 开始玩家回合
    }

    // 从CSV表格读取所有卡牌
    void LoadCardsFromCSV()
    {
        try
        {
            var file = FileAccess.Open("res://Data/Cards.csv", FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr("卡牌CSV文件不存在！路径：res://Data/Cards.csv");
                return;
            }

            file.GetCsvLine(); // 跳过标题

            while (!file.EofReached())
            {
                var line = file.GetCsvLine();
                if (line == null || line.Length < 5) continue;

                if (!int.TryParse(line[1], out int cost) || 
                    !int.TryParse(line[2], out int attack) || 
                    !int.TryParse(line[3], out int defense))
                {
                    continue;
                }

                CardData data = new CardData();
                data.CardName = line[0];
                data.Cost = cost;
                data.Attack = attack;
                data.Defense = defense;
                data.Desc = line[4];

                DrawPile.Add(data);
            }

            file.Close();
        }
        catch (Exception e)
        {
            GD.PrintErr($"加载卡牌CSV失败：{e.Message}");
        }
    }

    // ======================================================================
    // ✅ 洗牌逻辑（已经写在这里！正常打乱抽牌堆）
    // ======================================================================
    public void Shuffle()
    {
        Random random = new Random();
        DrawPile = DrawPile.OrderBy(x => random.Next()).ToList();
    }

    // 开始玩家回合
    public void StartPlayerTurn()
    {
        if (IsBattleEnd) return;

        CurrentEnergy = MaxEnergy;
        DrawCards(DrawCardCountPerTurn);

        foreach (var card in HandCards)
        {
            if (card != null)
                card.MouseFilter = Control.MouseFilterEnum.Stop;
        }
    }

    // 抽卡逻辑
    public void DrawCards(int count)
    {
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            if (DrawPile.Count == 0)
            {
                if (DiscardPile.Count == 0) break;

                DrawPile.AddRange(DiscardPile);
                DiscardPile.Clear();
                Shuffle();
            }

            var cardData = DrawPile[DrawPile.Count - 1];
            DrawPile.RemoveAt(DrawPile.Count - 1);

            var cardInstance = CardPrefab.Instantiate<Card>();
            cardInstance.Data = cardData;
            HandContainer.AddChild(cardInstance);
            HandCards.Add(cardInstance);
        }
    }

    // 打出卡牌
    public void PlayCard(Card card)
    {
        if (IsBattleEnd) return;
        if (card == null || card.Data == null) return;
        if (CurrentEnergy < card.Data.Cost) return;

        ExecuteCardEffect(card.Data);
        CurrentEnergy -= card.Data.Cost;

        DiscardPile.Add(card.Data);
        HandCards.Remove(card);
        card.QueueFree();

        bool hasPlayable = HandCards.Any(c => c.Data.Cost <= CurrentEnergy);
        if (!hasPlayable && CurrentEnergy == 0)
            EndPlayerTurn();
    }

    // 执行卡牌效果
    private void ExecuteCardEffect(CardData cardData)
    {
        if (cardData.Attack > 0)
            Enemy.TakeDamage(cardData.Attack);

        if (cardData.Defense > 0)
            Player.Heal(cardData.Defense);
    }

    // 结束玩家回合
    public void EndPlayerTurn()
    {
        if (IsBattleEnd) return;

        foreach (var card in HandCards)
        {
            if (card != null)
                card.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        GetTree().CreateTimer(1.0f).Timeout += () =>
        {
            Enemy.DoAction();

            if (Player.CurrentHp <= 0)
                return;

            GetTree().CreateTimer(1.0f).Timeout += StartPlayerTurn;
        };
    }

    // 胜利
    public void GameWin()
    {
        IsBattleEnd = true;
        GetTree().Paused = true;
    }

    // 失败
    public void GameLose()
    {
        IsBattleEnd = true;
        GetTree().Paused = true;
    }

    public override void _ExitTree()
    {
        Instance = null;
        DrawPile.Clear();
        HandCards.Clear();
        DiscardPile.Clear();
    }
}