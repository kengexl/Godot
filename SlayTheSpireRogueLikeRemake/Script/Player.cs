using Godot;

/// <summary>
/// 玩家角色脚本
/// 继承 CharacterBase，获得基础的血量、受伤、死亡逻辑
/// </summary>
public partial class Player : CharacterBase
{
    /// <summary>
    /// 玩家死亡时触发
    /// </summary>
    public override void Die()
    {
        // 打印调试信息
        GD.Print("玩家死亡！游戏结束！");
        
        // 通知 BattleManager 游戏失败
        BattleManager.Instance.GameLose();
        
        // 发射死亡信号（如果有死亡动画、UI可以监听这个信号）
        EmitSignal(SignalName.Died);
    }

    /// <summary>
    /// 玩家受伤时可以额外加逻辑（比如受伤抖动、播放音效）
    /// </summary>
    public override void TakeDamage(int damage)
    {
        // 先执行基类的受伤逻辑
        base.TakeDamage(damage);
        
        // 这里可以加受伤特效，比如屏幕抖动、播放受伤音效
        GD.Print($"玩家受到 {damage} 点伤害！");
    }

    /// <summary>
    /// 玩家回血方法（给治疗卡调用）
    /// </summary>
    public override void Heal(int value)
    {
        // 不能超过最大血量
        CurrentHp = Mathf.Min(MaxHp, CurrentHp + value);
        GD.Print($"玩家恢复 {value} 点血量！当前血量：{CurrentHp}/{MaxHp}");
        
        // 发射血量变化信号，更新UI
        EmitSignal(SignalName.HpChanged);
    }
}