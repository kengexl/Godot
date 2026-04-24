using Godot;

/// <summary>
/// 怪物基类脚本
/// 所有怪物（史莱姆、野狼、BOSS）都继承这个类
/// 提供基础的攻击、死亡、AI逻辑
/// </summary>
public partial class EnemyBase : CharacterBase
{
    [Export] public string EnemyName = "怪物";  // 怪物名字（编辑器可改）
    [Export] public int AttackPower = 5;       // 怪物攻击力（编辑器可改）
    [Export] public EnemyData EnemyData;       // 从配置文件读取怪物数据

    public override void _Ready()
    {
        base._Ready();
        
        // 如果配置了EnemyData，覆盖基础属性
        if (EnemyData != null)
        {
            EnemyName = EnemyData.EnemyName;
            MaxHp = EnemyData.HP;
            AttackPower = EnemyData.Atk;
            CurrentHp = MaxHp; // 重新初始化血量
            GD.Print($"📌 怪物数据加载：{EnemyName} HP:{MaxHp} ATK:{AttackPower}");
        }
    }

    /// <summary>
    /// 怪物死亡时触发
    /// </summary>
    public override void Die()
    {
        GD.Print($"{EnemyName} 被击败！战斗胜利！");
        
        // 通知 BattleManager 游戏胜利
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.GameWin();
        }
        
        // 发射死亡信号（可以用来播放死亡动画、掉落奖励）
        EmitSignal(SignalName.Died);
    }

    /// <summary>
    /// 怪物AI行动（回合结束时自动调用）
    /// 子类可以重写这个方法，实现自定义攻击/技能
    /// </summary>
    public virtual void DoAction()
    {
        if (BattleManager.Instance?.Player == null) return;
        
        GD.Print($"{EnemyName} 发动攻击！");
        
        // 对玩家造成伤害
        BattleManager.Instance.Player.TakeDamage(AttackPower);
    }

    /// <summary>
    /// 怪物受伤时可以额外加逻辑（比如受伤动画）
    /// </summary>
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        GD.Print($"{EnemyName} 受到 {damage} 点伤害！剩余血量：{CurrentHp}/{MaxHp}");
    }
}