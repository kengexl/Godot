using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Control = Godot.Control;

public partial class BattleManager : Node2D
{
    public static BattleManager Instance;

    [Export] public Player Player;
    [Export] public EnemyBase Enemy;
    [Export] public Container HandContainer;
    [Export] public PackedScene CardPrefab;

    [Export] public DrawPileUI DrawPile;
    [Export] public DiscardPileUI DiscardPile;
     [Export] public HealthBar PlayerHealthBar;
    [Export] public HealthBar EnemyHealthBar;


    [Export] public Label LabelCost;
    [Export] public Label LabelDebug1;
    [Export] public Label LabelDebug2;
    

    public List<Card> HandCards = new();
    public int CurrentEnergy;
    [Export] public int MaxEnergy = 3;
    [Export] public int DrawCardCountPerTurn = 3;
    public bool IsBattleEnd;

   public override void _Ready()
{
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;

    GD.Print("=== BattleManager._Ready 开始 ===");

    // 【新增】关键节点校验
    if (Player == null || Enemy == null || HandContainer == null || CardPrefab == null || DrawPile == null || DiscardPile == null)
    {
        PrintDebug("错误", "关键节点未配置");
        GD.PrintErr("❌ 关键节点未配置！");
        return;
    }

    // 【新增】强制等待Player初始化完成
    CallDeferred(nameof(DeferredBattleInit));
}

// 【新增】延迟初始化战斗
private void DeferredBattleInit()
{
    GD.Print("=== 延迟初始化战斗 ===");
    GD.Print($"Player当前血量：{Player.CurrentHp}/{Player.MaxHp}");

    // 【新增】如果玩家血量还是0，强制设置为满血
    if (Player.CurrentHp <= 0)
    {
        Player.CurrentHp = Player.MaxHp;
        Player.GlobalSavedCurrentHp = Player.MaxHp;
        GD.PrintErr($"⚠️ 玩家血量异常！已强制设为满血：{Player.CurrentHp}");
        // 强制同步UI
        Player.EmitSignal(CharacterBase.SignalName.HpChanged, Player.CurrentHp, Player.MaxHp);
    }

    LoadCustomDeck();
    InitHealthBars();
    StartPlayerTurn();
    
    GD.Print("=== BattleManager初始化完成 ===");
}
    
    // 新增：初始化血条
   private void InitHealthBars()
    {
        // 延迟一帧，确保Player和Enemy的_Ready先执行
        CallDeferred(nameof(DeferredInitHealthBars));
    }
private void DeferredInitHealthBars()
{
    // 玩家血条初始化
    if (Player != null && PlayerHealthBar != null)
    {
        PlayerHealthBar.Init(Player.MaxHp, Player.CurrentHp);
        // 监听玩家血量变化
        Player.Connect(CharacterBase.SignalName.HpChanged, Callable.From<int, int>(OnPlayerHpChanged));
        GD.Print($"✅ 玩家血条初始化成功：{Player.CurrentHp}/{Player.MaxHp}");
    }
    else
    {
        GD.PrintErr("❌ Player 或 PlayerHealthBar 节点为空！");
    }

    // 敌人血条初始化（敌人每次新场景都重置血量）
    if (Enemy != null && EnemyHealthBar != null)
    {
        // 敌人不需要全局保存，每次新场景重置为最大值
        Enemy.CurrentHp = Enemy.MaxHp;
        EnemyHealthBar.Init(Enemy.MaxHp, Enemy.CurrentHp);
        // 监听敌人血量变化
        Enemy.Connect(CharacterBase.SignalName.HpChanged, Callable.From<int, int>(OnEnemyHpChanged));
        GD.Print($"✅ 敌人血条初始化成功：{Enemy.CurrentHp}/{Enemy.MaxHp}");
    }
    else
    {
        GD.PrintErr("❌ Enemy 或 EnemyHealthBar 节点为空！");
    }
}
    // 新增：玩家血量变化回调
    private void OnPlayerHpChanged(int currentHp, int maxHp)
    {
        PlayerHealthBar?.UpdateHealth(currentHp, maxHp);
    }

    // 新增：敌人血量变化回调
    private void OnEnemyHpChanged(int currentHp, int maxHp)
    {
        EnemyHealthBar?.UpdateHealth(currentHp, maxHp);
    }

    // 新增：加载自定义卡组（有选卡用自定义，无则用默认）
    private void LoadCustomDeck()
{
    try
    {
        // ✅ 确认每次都读取最新的 GlobalBattleDeck
        if (CardAllTestChoose.GlobalBattleDeck != null && CardAllTestChoose.GlobalBattleDeck.Count > 0)
        {
            PrintDebug("自定义卡组", $"加载成功：{CardAllTestChoose.GlobalBattleDeck.Count} 张");
            GD.Print($"【BattleManager】加载自定义卡组，卡牌列表：{string.Join("、", CardAllTestChoose.GlobalBattleDeck.Select(c => c.CardName))}");
            DrawPile.AddCardsRange(CardAllTestChoose.GlobalBattleDeck);
            return;
        }
        // 无自定义卡组 → 加载默认CSV
        LoadCardsFromCSV();
    }
    catch (Exception e)
    {
        PrintDebug("错误", e.Message);
        LoadCardsFromCSV();
    }
}


    // 原有默认卡组加载逻辑（保留）
    void LoadCardsFromCSV()
    {
        try
        {
            var file = FileAccess.Open("res://Data/Cards.csv", FileAccess.ModeFlags.Read);
            if (file == null) { PrintDebug("错误", "找不到 Cards.csv"); return; }
            file.GetCsvLine();

            while (!file.EofReached())
            {
                var line = file.GetCsvLine();
                if (line == null || line.Length < 5) continue;

                if (!int.TryParse(line[1], out int cost) ||
                    !int.TryParse(line[2], out int attack) ||
                    !int.TryParse(line[3], out int defense)) continue;

                CardData data = new CardData();
                data.CardName = line[0];
                data.Cost = cost;
                data.Attack = attack;
                data.Defense = defense;
                data.Desc = line[4];

                DrawPile.AddCard(data);
            }
            file.Close();
            PrintDebug("加载", "默认卡牌初始化完成");
        }
        catch (Exception e) { PrintDebug("错误", e.Message); }
    }

    // 以下代码完全不变，保留你原有所有逻辑
    public List<CardData> Shuffle(List<CardData> list)
    {
        Random rand = new Random();
        return list.OrderBy(a => rand.Next()).ToList();
    }

    public void StartPlayerTurn()
    {
        if (IsBattleEnd) 
        {
            GD.Print("❌ 战斗已结束，跳过回合开始");
            return;
        }

        // 【新增】检查玩家是否已经死亡，防止能打牌但不显示失败
        if (Player.CurrentHp <= 0)
        {
            GD.Print("❌ 玩家血量已归0，触发失败");
            GameLose();
            return;
        }

        CurrentEnergy = MaxEnergy;
        RefreshCostUI();
        DrawCards(DrawCardCountPerTurn);
        
        foreach (var card in HandCards)
        {
            if (card != null)
                card.MouseFilter = Control.MouseFilterEnum.Stop;
        }
        
        PrintDebug("回合", "玩家回合开始");
    }

   // 【修改】抽卡逻辑，增加战斗结束检查
    public void DrawCards(int total)
    {
        if (IsBattleEnd) return;

        int remaining = total;
        while (remaining > 0)
        {
            if (DrawPile.GetCount() == 0 && DiscardPile.GetCount() > 0)
            {
                var discardCards = DiscardPile.GetAllCards();
                discardCards = Shuffle(discardCards);
                DrawPile.Clear();
                DrawPile.AddCardsRange(discardCards);
                DiscardPile.ClearAll();
                PrintDebug("补抽", "弃牌堆 → 抽牌堆");
            }
            if (DrawPile.GetCount() == 0)
                break;
            CardData data = DrawPile.DrawLastCard();
            Card card = CardPrefab.Instantiate<Card>();
            card.Data = data;
            card.CurrentZone = Card.CardZone.Hand;
            HandContainer.AddChild(card);
            HandCards.Add(card);
            remaining--;
        }
    }


    public void PlayCard(Card card)
    {
        if (IsBattleEnd) return;
        if (card == null || card.Data == null) return;
        if (CurrentEnergy < card.Data.Cost) return;

        ExecuteCardEffect(card.Data);
        CurrentEnergy -= card.Data.Cost;
        RefreshCostUI();

        DiscardPile.AddCard(card.Data);
        HandCards.Remove(card);
        card.QueueFree();
    }

    private void ExecuteCardEffect(CardData data)
    {
        if (data.Attack > 0) Enemy.TakeDamage(data.Attack);
        if (data.Defense > 0) Player.Heal(data.Defense);
    }

    public void EndTurn()
    {
        if (IsBattleEnd) return;

        foreach (var card in HandCards.ToList())
        {
            if (card != null && card.Data != null)
            {
                DiscardPile.AddCard(card.Data);
                card.QueueFree();
            }
        }
        HandCards.Clear();
        
        foreach (var card in HandCards)
        {
            if (card != null)
                card.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        PrintDebug("回合", "玩家回合结束");

        GetTree().CreateTimer(1.0f).Timeout += () =>
        {
            if (IsBattleEnd) return;
            Enemy.DoAction();
            
            if (Player.CurrentHp <= 0) 
            {
                GD.Print("❌ 玩家被敌人击败，触发失败");
                return;
            }

            GetTree().CreateTimer(1.0f).Timeout += StartPlayerTurn;
        };
    }

   public void GameWin()
{
    GD.Print("✅【DEBUG-1】GameWin 胜利方法被触发！");
    IsBattleEnd = true;
    GetTree().Paused = true;
    PrintDebug("战斗", "胜利！");

    // 调用选卡面板
    ShowVictoryPanel();
}

private void ShowVictoryPanel()
{
    GD.Print("✅【DEBUG-2】开始加载胜利面板 (Main场景内实例化)");
    
    // 1. 获取胜利面板场景（注意：这里路径必须和你文件系统里的一致！）
    // 右键点击你的胜利面板tscn文件 -> 复制路径，粘贴到下面
    string panelScenePath = "res://Scenes/victory_panel.tscn"; 
    
    PackedScene victoryScene = GD.Load<PackedScene>(panelScenePath);
    
    if (victoryScene == null)
    {
        GD.PrintErr($"❌【DEBUG-ERROR】路径错误！找不到文件：{panelScenePath}");
        return;
    }
    GD.Print("✅【DEBUG-3】胜利面板场景加载成功");

    // 2. 实例化面板
    Control victoryPanel = victoryScene.Instantiate<Control>();
    if (victoryPanel == null)
    {
        GD.PrintErr("❌【DEBUG-ERROR】实例化失败！根节点不是Control");
        return;
    }
    
    // 3. 找到Main场景里的 UILayer 并添加进去
    // 这里的 "UILayer" 必须和你第一步在Main场景里取的名字一模一样
    Node uiLayer = GetNodeOrNull("/root/Main/UILayer"); 
    
    // 容错写法：如果上面绝对路径找不到，尝试在当前场景下找
    if (uiLayer == null)
    {
        uiLayer = GetTree().CurrentScene.GetNodeOrNull("UILayer");
    }

    if (uiLayer == null)
    {
        GD.PrintErr("❌【DEBUG-ERROR】Main场景里找不到名为 'UILayer' 的节点！");
        return;
    }

    // 4. 加入场景
    uiLayer.AddChild(victoryPanel);
    GD.Print("✅【DEBUG-5】面板已添加到 Main.UILayer，执行完成！");
}
   // 【修改】游戏失败，确保逻辑正确
    public void GameLose()
    {
        if (IsBattleEnd) return;
        
        IsBattleEnd = true;
        GetTree().Paused = false;
        PrintDebug("战斗", "失败！");
        GD.Print("✅ 游戏失败逻辑触发");

        HandCards.Clear();
        DrawPile.Clear();
        DiscardPile.ClearAll();
        Instance = null;

        string targetScenePath = "res://Scenes/Card_All_Test_Choose.tscn";
        Error changeSceneErr = GetTree().ChangeSceneToFile(targetScenePath);
        if (changeSceneErr != Error.Ok)
        {
            GD.PrintErr($"❌ 切换场景失败：{changeSceneErr}");
        }
    }


    void RefreshCostUI()
    {
        if (LabelCost != null)
            LabelCost.Text = $"费用：{CurrentEnergy}/{MaxEnergy}";
    }

    void PrintDebug(string title, string msg)
    {
        if (LabelDebug1 != null) LabelDebug1.Text = title;
        if (LabelDebug2 != null) LabelDebug2.Text = msg;
    }

    public override void _ExitTree()
    {
        Instance = null;
        HandCards.Clear();
    }
}