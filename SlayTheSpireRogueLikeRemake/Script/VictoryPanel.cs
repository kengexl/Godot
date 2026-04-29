using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class VictoryPanel : Control
{
    [Export] public HBoxContainer CardContainer;
    [Export] public Button NextLevelButton;
    [Export] public PackedScene CardPrefab;

    private CardData _selectedCard;
    private List<Card> _spawnedCards = new List<Card>();

    public override void _Ready()
    {
        GD.Print("【Victory-Debug】=== VictoryPanel 初始化开始 ===");
        
        // 1. 设置暂停时也能运行
        ProcessMode = ProcessModeEnum.Always;
        GD.Print("【Victory-Debug】✅ 面板ProcessMode已设为Always");

        // 2. 节点校验
        if (CardContainer == null) GD.PrintErr("【Victory-Debug】❌ CardContainer 未赋值！");
        if (NextLevelButton == null) GD.PrintErr("【Victory-Debug】❌ NextLevelButton 未赋值！");
        if (CardPrefab == null) GD.PrintErr("【Victory-Debug】❌ CardPrefab 未赋值！");

        if (CardContainer == null || NextLevelButton == null || CardPrefab == null)
        {
            GD.PrintErr("【Victory-Debug】❌ 初始化终止：缺少关键节点");
            return;
        }

        // 3. 按钮初始化
        NextLevelButton.ProcessMode = ProcessModeEnum.Always;
        NextLevelButton.Pressed += OnNextLevelPressed;
        
        // ✅ 核心修复1：每次实例化都彻底重置所有状态
        ResetPanelState();
        
        // 4. 生成奖励卡
        PopulateRewardCards();
        GD.Print("【Victory-Debug】=== VictoryPanel 初始化完成 ===");
    }

    // ✅ 新增：彻底重置面板状态
    private void ResetPanelState()
    {
        _selectedCard = null;
        _spawnedCards.Clear();
        
        // 清空容器里的旧卡牌
        if (CardContainer != null)
        {
            foreach (var child in CardContainer.GetChildren())
            {
                child.QueueFree();
            }
        }
        
        // 重置按钮
        if (NextLevelButton != null)
        {
            NextLevelButton.Disabled = true;
        }
        
        GD.Print("【Victory-Debug】✅ 面板状态已彻底重置");
    }

    private void PopulateRewardCards()
    {
        GD.Print("【Victory-Debug】--- 开始生成奖励卡牌 ---");
        
        // 确保全局卡牌缓存不为空
        List<CardData> allCards = CardAllTestChoose.GlobalAllCardsCache;
        if (allCards == null || allCards.Count == 0)
        {
            GD.PrintErr("【Victory-Debug】❌ GlobalAllCardsCache 为空！重新加载CSV");
            allCards = CsvReader.ReadCardsFromCsv("res://Data/cards.csv");
            CardAllTestChoose.GlobalAllCardsCache = new List<CardData>(allCards);
        }
        
        if (allCards.Count == 0)
        {
            GD.PrintErr("【Victory-Debug】❌ 没有可用的卡牌数据！");
            return;
        }
        
        GD.Print($"【Victory-Debug】✅ 可用卡牌总数：{allCards.Count}");

        // 每次随机取3张（允许重复）
        var random = new RandomNumberGenerator();
        random.Randomize();
        var rewardCards = allCards.OrderBy(x => random.Randi()).Take(3).ToList();
        GD.Print($"【Victory-Debug】✅ 随机选出的3张卡：{string.Join("、", rewardCards.Select(c => c.CardName))}");

        // 生成新卡牌UI
        foreach (var cardData in rewardCards)
        {
            GD.Print($"【Victory-Debug】正在实例化卡牌：{cardData.CardName}");
            
            Card card = CardPrefab.Instantiate<Card>();
            if (card == null)
            {
                GD.PrintErr($"【Victory-Debug】❌ 卡牌实例化失败：{cardData.CardName}");
                continue;
            }

            // 卡牌设置
            card.ProcessMode = ProcessModeEnum.Always;
            card.SetCardData(cardData);
            card.DisableBattleClick();
            card.MouseFilter = MouseFilterEnum.Stop;
            card.MouseDefaultCursorShape = CursorShape.PointingHand;
            card.Visible = true;
            card.Modulate = new Color(0.6f, 0.6f, 0.6f); // 初始半透明

            // 绑定点击事件（闭包传递，无编译报错）
            card.GuiInput += (evt) => OnCardInput(evt, cardData, card);

            CardContainer.AddChild(card);
            _spawnedCards.Add(card);
            GD.Print($"【Victory-Debug】✅ 卡牌添加到容器：{cardData.CardName}");
        }
        
        GD.Print($"【Victory-Debug】--- 奖励卡牌生成结束，共生成 {_spawnedCards.Count} 张 ---");
    }

    private void OnCardInput(InputEvent evt, CardData cardData, Card cardUi)
    {
        GD.Print($"【Victory-Debug】>>> OnCardInput 被触发！目标卡牌：{cardData?.CardName ?? "NULL"}");
        
        // 严格过滤左键按下
        if (evt is not InputEventMouseButton mb) return;
        if (mb.ButtonIndex != MouseButton.Left || !mb.Pressed) return;

        GD.Print($"【Victory-Debug】✅ 有效点击！选中卡牌：{cardData.CardName}");

        // 高亮逻辑
        foreach (var c in _spawnedCards)
        {
            c.Modulate = new Color(0.6f, 0.6f, 0.6f); // 未选中半透明
        }
        cardUi.Modulate = Colors.White; // 选中高亮

        // 赋值选中卡
        _selectedCard = cardData;
        NextLevelButton.Disabled = false;
        GD.Print($"【Victory-Debug】✅ 下一关按钮已启用，选中卡：{_selectedCard.CardName}");
    }

    private void OnNextLevelPressed()
    {
        GD.Print("【Victory-Debug】>>> OnNextLevelPressed 被触发！");
        
        // 1. 校验选中卡
        if (_selectedCard == null)
        {
            GD.PrintErr("【Victory-Debug】❌ 未选中任何卡牌！");
            return;
        }

        GD.Print($"【Victory-Debug】准备添加卡牌到卡组：{_selectedCard.CardName}");
        GD.Print($"【Victory-Debug】添加前卡组总数：{CardAllTestChoose.GlobalBattleDeck.Count}");

        // ✅ 核心修复2：移除去重检查，允许重复添加同名卡牌！
        // 直接添加，不再判断 Contains
        CardAllTestChoose.GlobalBattleDeck.Add(_selectedCard);
        
        GD.Print($"【Victory-Debug】✅ 卡牌已添加！添加后卡组总数：{CardAllTestChoose.GlobalBattleDeck.Count}");
        GD.Print($"【Victory-Debug】当前卡组列表：{string.Join("、", CardAllTestChoose.GlobalBattleDeck.Select(c => c.CardName))}");

        // 2. 恢复暂停并切换场景
        GetTree().Paused = false;
        string mainScenePath = "res://Scenes/Main.tscn";
        
        if (ResourceLoader.Exists(mainScenePath))
        {
            GD.Print("【Victory-Debug】✅ 场景存在，执行切换");
            GetTree().ChangeSceneToFile(mainScenePath);
        }
        else
        {
            GD.PrintErr($"【Victory-Debug】❌ 场景路径错误：{mainScenePath}");
            GetTree().ReloadCurrentScene();
        }
    }

    // 清理资源
    public override void _ExitTree()
    {
        GD.Print("【Victory-Debug】=== VictoryPanel 被销毁，清理资源 ===");
        NextLevelButton.Pressed -= OnNextLevelPressed;
        foreach (var card in _spawnedCards)
        {
            card.QueueFree();
        }
        _spawnedCards.Clear();
        base._ExitTree();
    }
}