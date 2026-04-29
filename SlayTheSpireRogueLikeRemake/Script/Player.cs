using Godot;

public partial class Player : CharacterBase
{
    // 【修改】初始值改为更安全的-999，避免和任何可能的血量值冲突
    public static int GlobalSavedCurrentHp = -999;

    public override void _Ready()
    {
        GD.Print("=== Player._Ready 开始 ===");
        GD.Print($"1. GlobalSavedCurrentHp 初始值：{GlobalSavedCurrentHp}");
        GD.Print($"2. MaxHp 编辑器配置值：{MaxHp}");

        // 【修改1】强制校验MaxHp，防止编辑器中被设为0
        if (MaxHp <= 0)
        {
            MaxHp = 100; // 强制设置默认值
            GD.PrintErr($"⚠️ MaxHp配置错误！已强制设为默认值：{MaxHp}");
        }

        // 【修改2】更安全的初始化判断
        if (GlobalSavedCurrentHp == -999)
        {
            GlobalSavedCurrentHp = MaxHp;
            GD.Print($"✅ 首次游戏，强制设置满血：{GlobalSavedCurrentHp}");
        }
        // 【修改3】防止全局保存的血量异常（比如超过MaxHp或为负数）
        else if (GlobalSavedCurrentHp > MaxHp || GlobalSavedCurrentHp < 0)
        {
            GlobalSavedCurrentHp = MaxHp;
            GD.PrintErr($"⚠️ 保存的血量异常！已重置为满血：{GlobalSavedCurrentHp}");
        }

        // 赋值给当前角色
        CurrentHp = GlobalSavedCurrentHp;
        GD.Print($"3. 最终玩家血量：{CurrentHp}/{MaxHp}");

        // 强制发射信号，同步UI
        EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
        GD.Print("=== Player._Ready 结束 ===");
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        GlobalSavedCurrentHp = CurrentHp;
        GD.Print($"玩家受伤！保存血量：{GlobalSavedCurrentHp}");
    }

    public override void Heal(int value)
    {
        base.Heal(value);
        GlobalSavedCurrentHp = CurrentHp;
        GD.Print($"玩家回血！保存血量：{GlobalSavedCurrentHp}");
    }

    public override void Die()
    {
        GD.Print("玩家死亡！重置全局血量");
        GlobalSavedCurrentHp = -999; // 重置为初始标记值
        
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.GameLose();
        }
        
        base.Die();
    }
}