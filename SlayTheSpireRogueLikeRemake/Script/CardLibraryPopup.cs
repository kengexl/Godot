using Godot;
using System.Collections.Generic;

public partial class CardLibraryPopup : CanvasLayer
{
    public static CardLibraryPopup Instance;

    [Export] public Panel background;
    [Export] public Panel cardPanel;
    [Export] public GridContainer grid;
    [Export] public PackedScene cardPrefab;
    [Export] public Label titleLabel;

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
    }

    public void ShowCards(List<CardData> cards, string title)
    {
        ClearCards();

        titleLabel.Text = title;

        foreach (var data in cards)
        {
            var card = cardPrefab.Instantiate<Card>();
            card.Data = data;
            card.MouseFilter = Control.MouseFilterEnum.Ignore;
            grid.AddChild(card);
        }

        Visible = true;
    }

    public void ClosePopup()
    {
        Visible = false;
        ClearCards();
    }

    void ClearCards()
    {
        foreach (var c in grid.GetChildren())
            c.QueueFree();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn && btn.Pressed)
        {
            if (!cardPanel.GetGlobalRect().HasPoint(btn.GlobalPosition))
                ClosePopup();
        }
    }
}