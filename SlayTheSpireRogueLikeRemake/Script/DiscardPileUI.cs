using Godot;
using System.Collections.Generic;

public partial class DiscardPileUI : Node2D
{
    public static DiscardPileUI Instance { get; private set; }
    private List<CardData> _discardCards = new();

    [Export] public Label LabelDiscardCount;

    public override void _Ready()
    {
        Instance = this;
        RefreshCountUI();
    }

    public void AddCard(CardData card)
    {
        if (card == null) return;
        _discardCards.Add(card);
        RefreshCountUI();
    }

    public List<CardData> GetAllCards() => new List<CardData>(_discardCards);
    public void ClearAll() { _discardCards.Clear(); RefreshCountUI(); }
    public int GetCount() => _discardCards.Count;

    private void RefreshCountUI()
    {
        if (LabelDiscardCount != null)
            LabelDiscardCount.Text = $"弃牌堆：{_discardCards.Count}";
    }
}