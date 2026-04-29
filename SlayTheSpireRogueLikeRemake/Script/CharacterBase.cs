using Godot;

public partial class CharacterBase : Node2D
{
    [Signal]
    public delegate void HpChangedEventHandler(int currentHp, int maxHp);

    [Signal]
    public delegate void DiedEventHandler();

    [Export] public int MaxHp = 100;
    public int CurrentHp;

    // 【移除】全局静态变量移到Player类，避免Enemy干扰

    public override void _Ready()
    {
        // 【修改】基类只负责简单初始化，玩家的跨场景逻辑交给Player自己处理
        CurrentHp = MaxHp;
        GD.Print($"✅ 角色基类初始化：{CurrentHp}/{MaxHp}");
    }

    public virtual void TakeDamage(int damage)
    {
        if (CurrentHp <= 0) return;
        
        CurrentHp = Mathf.Max(0, CurrentHp - damage);
        EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
        
        if (CurrentHp <= 0) Die();
    }

    public virtual void Heal(int value)
    {
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + value);
        EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
    }

    public virtual void Die()
    {
        EmitSignal(SignalName.Died);
    }
}