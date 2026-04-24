using Godot;
using System;

public partial class EnemyData : Resource
{
    [Export] public string EnemyName;  // 修正拼写错误 EnrmyName -> EnemyName
    [Export] public int HP;  // 生命
    [Export] public int Atk;  // 伤害值
}