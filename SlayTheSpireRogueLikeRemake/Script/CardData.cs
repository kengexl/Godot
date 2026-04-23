using Godot;
using System;

public partial class CardData : Resource
{
    [Export] public string CardName; //卡片名字
    [Export] public int Cost; //卡片的费用
    [Export] public int Attack; //卡片的攻击
    [Export] public string Desc; 
}
