using Godot;
using System.Collections.Generic;

/// <summary>
/// 弃牌堆专属浏览面板（零报错+防拉伸最终版）
/// </summary>
public partial class DiscardPileBrowserPanel : Node2D
{
    private bool _isVisible = false;

    private Node2D _panelTargetPos;
    private Node2D _panelHidePos;

    [Export] public ColorRect Background;
    [Export] public Sprite2D Frame;
    [Export] public Container CardList;

    [Export] public float MoveSpeed = 8f;

    // 卡牌固定大小（兜底防拉伸，可在编辑器里直接调整）
    [Export] public Vector2 CardFixedSize = new Vector2(200, 300);

    public override void _Ready()
    {
        GD.Print("=== 弃牌堆面板 初始化成功 ===");

        _panelTargetPos = GetNodeOrNull<Node2D>("PanelTargetPos");
        _panelHidePos = GetNodeOrNull<Node2D>("PanelHidePos");

        if (_panelTargetPos != null)
            Position = _panelTargetPos.Position;

        Visible = false;

        if (Background != null)
            Background.GuiInput += OnBackgroundClicked;
    }

    public override void _Process(double delta)
    {
        if (_panelTargetPos == null || _panelHidePos == null)
            return;

        var targetPos = _isVisible ? _panelTargetPos.Position : _panelHidePos.Position;
        Position = Position.Lerp(targetPos, MoveSpeed * (float)delta);
    }

    #region 独立公开方法（按钮绑定）
    public void OpenDiscardPanel()
    {
        GD.Print("✅ 弃牌堆面板 打开");
        _isVisible = true;
        Visible = true;
        ShowDiscardCards();
    }

    public void CloseDiscardPanel()
    {
        GD.Print("❌ 弃牌堆面板 关闭");
        _isVisible = false;
        Visible = false;
        ClearCards();
    }
    #endregion

    #region 弃牌堆逻辑（零报错+防拉伸）
    private void ShowDiscardCards()
    {
        ClearCards();

        if (DiscardPileUI.Instance == null)
        {
            GD.PrintErr("❌ 弃牌堆UI未找到！");
            return;
        }

        List<CardData> cardDatas = DiscardPileUI.Instance.GetAllCards();
        if (cardDatas == null || cardDatas.Count == 0)
        {
            GD.Print("ℹ️ 弃牌堆为空");
            return;
        }

        PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
        if (cardScene == null)
        {
            GD.PrintErr("❌ 卡牌预制体路径错误！");
            return;
        }

        foreach (var data in cardDatas)
        {
            if (data == null) continue;

            try
            {
                Node instance = cardScene.Instantiate();
                if (!(instance is Card card))
                {
                    instance.QueueFree();
                    continue;
                }

                card.SetCardData(data);

                // 🔥 双重保险防拉伸（兼容所有Godot 4版本）
                if (card is Control controlCard)
                {
                    // 1. 设置SizeFlags为收缩居中（通用写法，不会报错）
                    try
                    {
                        controlCard.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
                        controlCard.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
                    }
                    catch
                    {
                        // 极端版本兼容失败时，跳过，靠下面的固定大小兜底
                        GD.Print("⚠️ SizeFlags设置兼容跳过");
                    }

                    // 2. 强制设置固定最小大小，防止被容器拉伸（兜底方案）
                    controlCard.CustomMinimumSize = CardFixedSize;
                }

                CardList.AddChild(card);
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"❌ 卡牌加载异常：{ex.Message}");
            }
        }
    }
    #endregion

    #region 工具方法
    private void ClearCards()
    {
        if (CardList == null) return;

        foreach (var child in CardList.GetChildren())
        {
            if (child != null && !child.IsQueuedForDeletion())
                child.QueueFree();
        }
    }

    private void OnBackgroundClicked(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
        {
            CloseDiscardPanel();
        }
    }
    #endregion
}