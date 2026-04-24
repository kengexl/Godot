using Godot;

/// <summary>
/// 角色基类：玩家和怪物都继承这个类
/// 统一管理血量、受伤、死亡逻辑
/// </summary>
public abstract partial class CharacterBase : Node2D
{
    [Export] public int MaxHp = 30;  // 最大血量（编辑器可改）
    public int CurrentHp { get; protected set; } // 当前血量（只读，外部只能通过TakeDamage/Heal修改）

    // 血量变化信号（UI血条可以监听这个信号自动更新）
    [Signal] public delegate void HpChangedEventHandler();
    // 角色死亡信号（可以监听这个信号触发死亡逻辑）
    [Signal] public delegate void DiedEventHandler();

    public override void _Ready()
    {
        // 初始化当前血量为最大血量
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// 受伤方法
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        // 扣血，不能低于0
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        
        // 发射血量变化信号
        EmitSignal(SignalName.HpChanged);

        // 如果血量为0，触发死亡
        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 回血方法
    /// </summary>
    public virtual void Heal(int value)
    {
        // 回血，不能超过最大血量
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + value);
        EmitSignal(SignalName.HpChanged);
    }

    /// <summary>
    /// 死亡方法（必须由子类实现）
    /// </summary>
    public abstract void Die();
}