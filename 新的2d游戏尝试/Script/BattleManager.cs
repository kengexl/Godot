using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node2D
{
    public static BattleManager Instance {get;private set;}
    [Header("回合")]
    public int CurrentEnergy;
    public int MaxEnergy = 3;
    [Header("牌堆")]
    public List<CardData> DrawPile = new();
    public List<Card> HandCard = new();
    public List<CardData> DiscardPile = new();

    [Header("UI父节点")]
    [Export] public Container HandCotainer;
    public override void _Ready()
    {
        Instance = this;
        InitDefaultDeck();
        StarNewTurn();
    }

    void InitDefaultDeck()//初始化基础卡牌
    {
        for (int i = 0;i<10;i++)
        {
            DrawPile.Add
            (
                new CardData()
            {
                CardName = "打击",
                Cost = 1,
                Attack = 6,
                Desc = "造成6点伤害"
            }
            );
        }
        ShuffleDrawPile();
    }

    public void ShuffleDrawPile()//洗牌
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        DrawPile = DrawPile.OrderBy(_ => rng.Randf()).ToList();

    }

    //开始新回合
    public void StartNewTurn()
    {
        CurrentEnergy = MaxEnergy;
        DrawCard(5);
    }

    void SpawnCard(CardData data)
    {
        var cardPrefab = GD.Load<PackedScene>("res://Scenes/Card.tscn"); 
        Card newCard       

    }



    public void DrawCard(int count)
    {
        for (int i = 0;i<count;i++)
        {
            if (DrawPile.Count<=0)
            {
                DrawPile.AddRange(DiscardPile);
                DiscardPile.Clear();
                ShuffleDrawPile();
            }

            var data = DrawPile.First();
            DrawPile.RemoveAt(0);
            SpawnCard(data);
        }

    }
}
