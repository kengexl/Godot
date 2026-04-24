using Godot;

// 加上这个特性，让它能在编辑器里作为资源创建
[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string CardName;
    [Export] public int Cost;
    [Export] public int Attack;
    [Export] public int Defense;
    [Export] public string Desc;

    // 构造函数（可选，方便代码里创建）
    public CardData() { }
}