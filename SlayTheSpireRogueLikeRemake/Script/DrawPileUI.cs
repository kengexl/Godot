using Godot;
using System.Collections.Generic;

/// <summary>
/// 抽牌堆 - 独立场景 （未来可随意扩展：查看牌堆、抽牌动画、数量显示）
/// </summary>
public partial class DrawPileUI : Node2D
{
    public static DrawPileUI Instance { get; private set; }

    // 核心卡牌列表
    private List<CardData> _drawCards = new List<CardData>();
    

    [Export] public Label LabelDrawCount;
    public List<CardData> GetAllCardDatas()
{
    // 防止外部修改，返回副本
    return new List<CardData>(_drawCards);
}

    public override void _Ready()
    {
        Instance = this;
        RefreshCountUI();
    }

    // ===================== 外部接口 =====================
    public void AddCard(CardData card)
    {
        if (card == null) return;
        _drawCards.Add(card);
        RefreshCountUI();
    }

    public void AddCardsRange(List<CardData> cards)
    {
        _drawCards.AddRange(cards);
        RefreshCountUI();
    }

    public CardData DrawLastCard()
    {
        if (_drawCards.Count == 0) return null;

        var card = _drawCards[^1];
        _drawCards.RemoveAt(_drawCards.Count - 1);
        RefreshCountUI();
        return card;
    }

    public void Clear()
    {
        _drawCards.Clear();
        RefreshCountUI();
    }

    public int GetCount() => _drawCards.Count;
    public List<CardData> GetAllCards() => new List<CardData>(_drawCards);

    // ===================== 工具 =====================
    private void RefreshCountUI()
    {
        if (LabelDrawCount != null)
            LabelDrawCount.Text = $"抽牌堆：{_drawCards.Count}";
    }
}