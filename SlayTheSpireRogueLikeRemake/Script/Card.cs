using Godot;

public partial class Card : Control
{
    [Export] public CardData Data;

    [Export] public Label NameLabel;
    [Export] public Label CostLabel;
    [Export] public Label DescLabel;

    public override void _Ready()
    {
         GD.Print("✅ Card脚本初始化成功"); // 加这句测试
        // 关键！绑定点击事件
        GuiInput += OnCardClick;

        // 刷新UI
        RefreshCardUI();
    }

    void RefreshCardUI()
    {
        if (Data == null) return;
        NameLabel.Text = Data.CardName;
        CostLabel.Text = Data.Cost.ToString();
        DescLabel.Text = Data.Desc;
    }

    void OnCardClick(InputEvent evt)
    {
        // 只响应鼠标左键按下
        if (evt is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
        {
            GD.Print($"[点击测试] 卡牌被点击：{Data?.CardName}");
            BattleManager.Instance.PlayCard(this);
        }
    }
}