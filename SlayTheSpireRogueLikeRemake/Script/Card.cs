using Godot;

public partial class Card : Control
{
    public CardData Data;
    private bool _isBattleClickEnabled = true;

    private Label _labelName;
    private Label _labelCost;
    private Label _labelAttack;
    private Label _labelDefense;
    private Label _labelDesc;

    public override void _Ready()
    {
        GD.Print($"【Card-Debug】卡牌节点初始化，Name: {Name}");
        
        _labelName = GetNode<Label>("Label_Name");
        _labelCost = GetNode<Label>("Label_Cost");
        _labelAttack = GetNode<Label>("Label_Attack");
        _labelDefense = GetNode<Label>("Label_Defense");
        _labelDesc = GetNode<Label>("Label_Desc");

        UpdateCardUI();
    }

    public void SetCardData(CardData cardData)
    {
        Data = cardData;
        GD.Print($"【Card-Debug】SetCardData 被调用，卡牌名: {cardData?.CardName ?? "NULL"}");
        UpdateCardUI();
    }

    private void UpdateCardUI()
    {
        if (Data == null)
        {
            GD.PrintErr("【Card-Debug】UpdateCardUI 失败：Data 为空！");
            return;
        }

        GD.Print($"【Card-Debug】更新UI：{Data.CardName} (费用:{Data.Cost} 攻击:{Data.Attack} 防御:{Data.Defense})");

        if (_labelName != null) _labelName.Text = Data.CardName;
        if (_labelCost != null) _labelCost.Text = Data.Cost.ToString();
        if (_labelAttack != null) _labelAttack.Text = Data.Attack.ToString();
        if (_labelDefense != null) _labelDefense.Text = Data.Defense.ToString();
        if (_labelDesc != null) _labelDesc.Text = Data.Desc;
    }

    public void DisableBattleClick()
    {
        _isBattleClickEnabled = false;
        GD.Print($"【Card-Debug】DisableBattleClick 被调用，战斗出牌已禁用 (当前卡牌: {Data?.CardName ?? "NULL"})");
    }

    public override void _GuiInput(InputEvent @event)
    {
        // 整个方法只声明一次mb变量，无编译报错
        InputEventMouseButton mb = @event as InputEventMouseButton;
        
        // 必打日志：只要进入这个方法，就会打印，证明输入事件被触发了
        if (mb != null)
        {
            GD.Print($"【Card-Debug】_GuiInput 被触发！卡牌={Data?.CardName ?? "NULL"}, 按钮={mb.ButtonIndex}, 按下={mb.Pressed}, 战斗模式={_isBattleClickEnabled}");
        }

        if (_isBattleClickEnabled)
        {
            // 战斗模式：出牌逻辑
            if (mb != null && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                GD.Print($"【Card-Debug】战斗模式：触发出牌！卡牌: {Data?.CardName}");
                BattleManager.Instance?.PlayCard(this);
            }
        }
        else
        {
            GD.Print($"【Card-Debug】胜利模式：事件透传，等待VictoryPanel处理");
        }
    }
}