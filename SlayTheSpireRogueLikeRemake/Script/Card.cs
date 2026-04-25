using Godot;

public partial class Card : Control
{
    public CardData Data;
    private bool _isBattleClickEnabled = true;

    private Label _labelName;
    private Label _labelCost;
    private Label _labelAttack;
    private Label _labelDefense;
    private Label _labelDesc;

    public override void _Ready()
    {
        // 节点初始化
        _labelName = GetNode<Label>("Label_Name");
        _labelCost = GetNode<Label>("Label_Cost");
        _labelAttack = GetNode<Label>("Label_Attack");
        _labelDefense = GetNode<Label>("Label_Defense");
        _labelDesc = GetNode<Label>("Label_Desc");

        // ✅ 关键修复：节点初始化完成后，再更新一次UI
        UpdateCardUI();
    }

    public void SetCardData(CardData cardData)
    {
        Data = cardData;
        UpdateCardUI();
    }

    private void UpdateCardUI()
    {
        if (Data == null) return;

        // ✅ 关键修复：给每个Label赋值加上空检查，防止节点未初始化时报错
        if (_labelName != null) _labelName.Text = Data.CardName;
        if (_labelCost != null) _labelCost.Text = Data.Cost.ToString();
        if (_labelAttack != null) _labelAttack.Text = Data.Attack.ToString();
        if (_labelDefense != null) _labelDefense.Text = Data.Defense.ToString();
        if (_labelDesc != null) _labelDesc.Text = Data.Desc;
    }

    public void DisableBattleClick()
    {
        _isBattleClickEnabled = false;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_isBattleClickEnabled) return;

        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            BattleManager.Instance.PlayCard(this);
        }
    }
}