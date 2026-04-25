using Godot;
using System.Collections.Generic;

/// <summary>
/// CSV 读取工具（Godot 4.5.1 专用）
/// </summary>
public static class CsvReader
{
    /// <summary>
    /// 从 CSV 文件读取所有卡牌数据
    /// </summary>
    /// <param name="filePath">文件路径，如 "res://Data/cards.csv"</param>
    /// <returns>卡牌数据列表</returns>
    public static List<CardData> ReadCardsFromCsv(string filePath)
    {
        List<CardData> resultList = new List<CardData>();

        // 1. 检查文件是否存在
        if (!FileAccess.FileExists(filePath))
        {
            GD.PrintErr($"[错误] CSV文件未找到: {filePath}");
            return resultList;
        }

        // 2. 打开文件 (Godot 4.x 标准写法)
        using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        
        if (file == null)
        {
            GD.PrintErr($"[错误] 无法打开文件: {FileAccess.GetOpenError()}");
            return resultList;
        }

        bool isHeaderLine = true;

        // 3. 逐行读取
        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            
            // 跳过空行和表头
            if (string.IsNullOrEmpty(line)) continue;
            if (isHeaderLine)
            {
                isHeaderLine = false;
                continue;
            }

            // 4. 简单解析 CSV (按逗号分割)
            string[] columns = line.Split(',');

            if (columns.Length >= 5)
            {
                try
                {
                    CardData newCard = new CardData(columns);
                    resultList.Add(newCard);
                    GD.Print($"[读取成功] 卡牌: {newCard.CardName}");
                }
                catch (System.Exception e)
                {
                    GD.PrintErr($"[解析失败] 行内容: {line}, 异常: {e.Message}");
                }
            }
        }

        GD.Print($"[CSV读取完成] 共获取 {resultList.Count} 张卡牌");
        return resultList;
    }
}