using Godot;

public partial class HealthBar : Control
{
    [Export] private ProgressBar _hpBar;   // 编辑器绑定HpBar节点
    [Export] private Label _hpLabel;       // 编辑器绑定HpLabel节点

    // 初始化血条（设置初始血量）
    public void Init(int maxHp, int currentHp)
    {
        UpdateHealth(currentHp, maxHp);
    }

    // 更新血量显示（核心方法）
    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (_hpBar == null || _hpLabel == null)
        {
            GD.PrintErr("血条节点未配置！检查HpBar/HpLabel绑定");
            return;
        }

        // 1. 更新数值文本
        _hpLabel.Text = $"{currentHp}/{maxHp}";

        // 2. 更新进度条（平滑动画过渡）
        _hpBar.MaxValue = maxHp;
        Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(_hpBar, "value", currentHp, 0.2f); // 0.2秒平滑变化

        // 3. 血量占比变色（低血量红、中血量橙、高血量绿）
        float hpRatio = (float)currentHp / maxHp;
        Color targetColor = hpRatio <= 0.2f ? Colors.Red : (hpRatio <= 0.5f ? Colors.Orange : Colors.Green);
        tween.TweenProperty(_hpBar, "modulate", targetColor, 0.2f);
    }

    // 自动获取节点（防止编辑器漏绑）
    public override void _Ready()
    {
        _hpBar ??= GetNode<ProgressBar>("HpBar");
        _hpLabel ??= GetNode<Label>("HpLabel");
        
        // 基础样式初始化（可选）
        _hpBar.Modulate = Colors.Green;
        _hpLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hpLabel.VerticalAlignment = VerticalAlignment.Center;
    }
}