using Godot;
using System;

public partial class EnemyData : Resource
{
    [Export] public string EnrmyName;  //怪物名字
    [Export] public int HP;  //生命
    [Export] public int Atk;  //伤害值
}
