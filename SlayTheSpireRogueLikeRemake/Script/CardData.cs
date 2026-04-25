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

    // 构造函数 1：无参构造（Godot 要求，必须保留）
    public CardData() { }

    // ---------------------------------------------------------
    // ✨ 新增部分：用于 CSV 读取的构造函数
    // ---------------------------------------------------------
    public CardData(string[] csvRow)
    {
        // 安全检查：确保列数足够
        if (csvRow.Length < 5)
        {
            GD.PrintErr("CSV行数据不足，使用默认值创建卡牌");
            CardName = "未知卡牌";
            Cost = 1;
            return;
        }

        // 赋值（带容错处理）
        CardName = csvRow[0].Trim();
        Cost = int.TryParse(csvRow[1].Trim(), out int c) ? c : 1;
        Attack = int.TryParse(csvRow[2].Trim(), out int a) ? a : 0;
        Defense = int.TryParse(csvRow[3].Trim(), out int d) ? d : 0;
        Desc = csvRow[4].Trim();
    }
}