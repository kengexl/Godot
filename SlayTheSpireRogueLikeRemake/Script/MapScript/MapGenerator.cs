using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace
{
    public partial class MapGenerator : Node
    {
            // 辅助类：用于追踪单条路径上的节点数量
    private class PathTracker
    {
        public int EliteCount = 0;
        public int TreasureCount = 0;
        public int RestCount = 0;
        public List<string> NodeIds = new();
    }
        // 地图生成完成信号
        [Signal] public delegate void OnMapGeneratedEventHandler();
        [Export] public MapGeneratorConfig Config;
        [ExportGroup("布局设置")]
        [Export] public float LayerVerticalSpacing = 150; // 每层之间的垂直距离
        [Export] public float NodeHorizontalSpacing = 200; // 同一层节点之间的水平距离
        [Export] public Vector2 MapCenterOffset = new Vector2(0, 0); // 整个地图的中心偏移
        
        
        // 输出数据
        public Dictionary<string, MapNode> Nodes = new(); // 节点字典
        public Dictionary<string, List<string>> Edges = new(); // 边字典 (Key: 源节点ID, Value: 目标ID列表)
        
        // 内部状态
        private int _nodeIdCounter = 0;
        private Random _random;
        // 👇 新增这行：存储每一层的节点数量规划
        private List<int> _layerNodeCounts = new(); 
         // 👇 新增：按层存储节点ID，方便后面连接
        private Dictionary<int, List<string>> _nodesByLayer = new(); 

        public override void _Ready()
        {
            _random = new Random();
            
            // 游戏启动时自动生成地图
            GenerateMap();
        }

        // 外部调用入口
        public void GenerateMap()
        {
            if (Config == null)
            {
                GD.PrintErr("请配置 MapGeneratorConfig!");
                return;
            }

            Nodes.Clear();
            Edges.Clear();
            _layerNodeCounts.Clear();
            _nodesByLayer.Clear();
            _nodeIdCounter = 0;

            if (!ValidateParameters()) return;
            CreateFixedNodes();
            PlanLayerNodeCounts();
            GenerateNodesByLayer();
            ConnectNodes();
            CalculateNodePositions();

            // 🆕 发射信号，通知所有监听者地图生成好了
            EmitSignal(SignalName.OnMapGenerated);
        }
        // ---------------------------------------------------------
        // 阶段0：校验参数
        // ---------------------------------------------------------
        private bool ValidateParameters()
        {
            int middleLayerCount = Config.TotalLayers - 2; // 减去起点和Boss层
            
            // 1. 检查层数是否足够
            if (Config.TargetTotalNodes < middleLayerCount)
            {
                GD.PrintErr($"生成失败：目标节点数({Config.TargetTotalNodes})小于最少需求({middleLayerCount})");
                return false;
            }

            // 2. 检查特殊节点是否超标
            int specialSum = Config.EliteCount + Config.TreasureCount + Config.RestCount;
            if (specialSum > Config.TargetTotalNodes)
            {
                GD.PrintErr($"生成失败：特殊节点总数({specialSum})超过目标节点数({Config.TargetTotalNodes})");
                return false;
            }

            GD.Print("✅ 参数校验通过");
            return true;
        }

        // ---------------------------------------------------------
        // 阶段1：创建固定节点 (起点和Boss)
        // ---------------------------------------------------------
        private void CreateFixedNodes()
        {
            // 创建起点 (Layer 0)
            var startNode = CreateNode(0, NodeType.Start);
            
            // 创建Boss (Layer 最后一层)
            var bossNode = CreateNode(Config.TotalLayers - 1, NodeType.Boss);
            
            GD.Print($"✅ 固定节点创建完毕: Start({startNode.Id}), Boss({bossNode.Id})");
        }
       // ---------------------------------------------------------
// 阶段2：金字塔式楼层节点规划（先增后减，无交叉必备）
// ---------------------------------------------------------
// ---------------------------------------------------------
// 阶段2：修复版金字塔式楼层节点规划
// ---------------------------------------------------------
private void PlanLayerNodeCounts()
{
    _layerNodeCounts.Clear();
    int totalMiddleLayers = Config.TotalLayers - 2; // 不含起点和Boss

    if (totalMiddleLayers <= 0)
    {
        GD.PrintErr("总层数太少，至少需要3层（起点+1层中间+Boss）");
        return;
    }

    GD.Print($"\n📐 开始金字塔式节点分配: 中间层数={totalMiddleLayers}");

    // 计算峰值位置和峰值节点数
    int peakPosition = totalMiddleLayers / 2;
    int maxNodes = Mathf.Min(Config.MaxNodesPerLayer, totalMiddleLayers + 1);
    int currentNodes = Config.MinNodesPerLayer;

    // 上升阶段：从最小值增加到峰值
    for (int i = 0; i <= peakPosition; i++)
    {
        _layerNodeCounts.Add(currentNodes);
        if (currentNodes < maxNodes)
        {
            currentNodes++;
        }
    }

    // 下降阶段：从峰值减少到最小值
    currentNodes = _layerNodeCounts.Last() - 1;
    for (int i = peakPosition + 1; i < totalMiddleLayers; i++)
    {
        _layerNodeCounts.Add(Mathf.Max(currentNodes, Config.MinNodesPerLayer));
        if (currentNodes > Config.MinNodesPerLayer)
        {
            currentNodes--;
        }
    }

    // 打印最终规划
    string log = "📊 金字塔楼层规划: [Start(1)] -> ";
    foreach (var count in _layerNodeCounts)
    {
        log += $"{count} -> ";
    }
    log += "[Boss(1)]";
    GD.Print(log);
}
        // ---------------------------------------------------------
        // 阶段3：逐层生成节点并分配类型
        // ---------------------------------------------------------
                // ---------------------------------------------------------
        // 阶段3：高级节点生成（带所有规则）
        // ---------------------------------------------------------
        private void GenerateNodesByLayer()
        {
            _nodesByLayer.Clear();
            
            // 初始化层字典
            for (int i = 0; i < Config.TotalLayers; i++)
                _nodesByLayer[i] = new List<string>();

            // 1. 创建固定节点
            var startNode = Nodes.Values.First(n => n.Type == NodeType.Start);
            var bossNode = Nodes.Values.First(n => n.Type == NodeType.Boss);
            _nodesByLayer[0].Add(startNode.Id);
            _nodesByLayer[Config.TotalLayers - 1].Add(bossNode.Id);

            GD.Print($"\n🎲 开始高级节点生成...");

            // 2. 先创建所有普通节点（占位）
            for (int i = 0; i < _layerNodeCounts.Count; i++)
            {
                int actualLayerIndex = i + 1;
                int nodeCountInThisLayer = _layerNodeCounts[i];
                
                for (int j = 0; j < nodeCountInThisLayer; j++)
                {
                    var newNode = CreateNode(actualLayerIndex, NodeType.Enemy); // 先全是小怪
                    _nodesByLayer[actualLayerIndex].Add(newNode.Id);
                }
            }

            // 3. 应用特殊规则：Boss前强制是篝火
            if (Config.ForceRestBeforeBoss)
            {
                int bossLayer = Config.TotalLayers - 1;
                int preBossLayer = bossLayer - 1;
                if (_nodesByLayer.ContainsKey(preBossLayer) && _nodesByLayer[preBossLayer].Count > 0)
                {
                    // 把Boss前一层的第一个节点强制设为篝火
                    string nodeId = _nodesByLayer[preBossLayer][0];
                    Nodes[nodeId].Type = NodeType.Rest;
                    GD.Print($"   🔥 Boss前强制篝火: {nodeId}");
                }
            }

            // 4. 智能分配精英、宝箱、篝火
            // 这里我们用简化但有效的方法：
            // - 精英尽量分散在不同层
            // - 宝箱不在前几层
            // - 篝火尽量在精英附近
            
            // 先收集所有中间层节点
            List<string> allMiddleNodes = new();
            for (int layer = 1; layer < Config.TotalLayers - 1; layer++)
            {
                allMiddleNodes.AddRange(_nodesByLayer[layer]);
            }

            // 打乱顺序，增加随机性
            allMiddleNodes = allMiddleNodes.OrderBy(x => _random.Next()).ToList();

            // 分配精英
            int elitesAssigned = 0;
            foreach (string nodeId in allMiddleNodes)
            {
                if (elitesAssigned >= Config.EliteCount) break;
                if (Nodes[nodeId].Type != NodeType.Enemy) continue; // 跳过已分配的
                
                Nodes[nodeId].Type = NodeType.Elite;
                elitesAssigned++;
            }

            // 分配宝箱（不在前几层）
            int treasuresAssigned = 0;
            foreach (string nodeId in allMiddleNodes)
            {
                if (treasuresAssigned >= Config.TreasureCount) break;
                if (Nodes[nodeId].Type != NodeType.Enemy) continue;
                if (Nodes[nodeId].Layer < Config.TreasureStartLayer) continue; // 前几层不出现
                
                Nodes[nodeId].Type = NodeType.Treasure;
                treasuresAssigned++;
            }

            // 分配篝火（优先在精英附近）
            int restsAssigned = 0;
            // 先找精英附近的空位
            foreach (string nodeId in allMiddleNodes)
            {
                if (restsAssigned >= Config.RestCount) break;
                if (Nodes[nodeId].Type != NodeType.Enemy) continue;
                
                // 检查这一层有没有精英
                bool hasEliteInLayer = _nodesByLayer[Nodes[nodeId].Layer]
                    .Any(id => Nodes[id].Type == NodeType.Elite);
                
                if (hasEliteInLayer && _random.NextDouble() < Config.RestNearEliteBonus)
                {
                    Nodes[nodeId].Type = NodeType.Rest;
                    restsAssigned++;
                }
            }
            // 剩下的篝火随机分配
            foreach (string nodeId in allMiddleNodes)
            {
                if (restsAssigned >= Config.RestCount) break;
                if (Nodes[nodeId].Type != NodeType.Enemy) continue;
                
                Nodes[nodeId].Type = NodeType.Rest;
                restsAssigned++;
            }

            // 5. 剩余节点随机填充
            foreach (string nodeId in allMiddleNodes)
            {
                if (Nodes[nodeId].Type != NodeType.Enemy) continue;
                
                var normalTypes = new[] { NodeType.Enemy, NodeType.Event, NodeType.Shop };
                Nodes[nodeId].Type = normalTypes[_random.Next(normalTypes.Length)];
            }
            
            GD.Print("✅ 高级节点生成完毕！");
        }
       // ---------------------------------------------------------
// 阶段4：杀戮尖塔原版无交叉连接算法
// 规则：当前层第i个节点，只能连接到下一层的i和i+1号节点
// ---------------------------------------------------------
// ---------------------------------------------------------
// 阶段4：修复版无交叉连接算法（解决左右列隔离问题）
// 核心改进：允许上一层第i个节点连接到下一层的i-1、i、i+1号节点
// 同时保证绝对不会出现交叉连线
// ---------------------------------------------------------
// ---------------------------------------------------------
// 阶段4：最终版无交叉连接算法（零交叉+全连通）
// 铁律：当前层第i个节点，只能连接到下一层的i和i+1号节点
// 这样绝对不会有任何交叉连线
// ---------------------------------------------------------
// ---------------------------------------------------------
// 阶段4：最终版无交叉连接算法（零交叉+全连通+边界修复）
// 铁律：当前层第i个节点，只能连接到下一层的i和i+1号节点
// 完美处理金字塔上升和下降阶段的所有边界情况
// ---------------------------------------------------------
private void ConnectNodes()
{
    GD.Print($"\n🔗 开始最终版无交叉连接节点 (边界修复)...");

    // 从第0层（起点）开始，逐层往上连接
    for (int layer = 0; layer < Config.TotalLayers - 1; layer++)
    {
        List<string> currentLayer = _nodesByLayer[layer];
        List<string> nextLayer = _nodesByLayer[layer + 1];
        
        GD.Print($"   连接层 {layer} ({currentLayer.Count}个) -> 层 {layer+1} ({nextLayer.Count}个)");

        // 第一步：基础连接（保证无交叉）
        for (int i = 0; i < currentLayer.Count; i++)
        {
            string srcNode = currentLayer[i];
            List<int> possibleTargets = new List<int>();

            // 🔥 核心铁律 + 边界处理
            if (i < nextLayer.Count)
            {
                // 正常情况：可以连正下方和右下方
                possibleTargets.Add(i);
                if (i + 1 < nextLayer.Count)
                {
                    possibleTargets.Add(i + 1);
                }
            }
            else
            {
                // 🔥 边界情况：当前层节点数 > 下一层
                // 最后一个节点只能连到下一层的最后一个节点
                possibleTargets.Add(nextLayer.Count - 1);
            }

            // 随机选择连接方式
            if (possibleTargets.Count == 1)
            {
                // 只有一个选择，必须连
                CreateEdge(srcNode, nextLayer[possibleTargets[0]]);
            }
            else
            {
                // 50%概率连一个，50%概率连两个
                if (_random.NextDouble() < 0.5)
                {
                    int chosenIndex = possibleTargets[_random.Next(possibleTargets.Count)];
                    CreateEdge(srcNode, nextLayer[chosenIndex]);
                }
                else
                {
                    // 两个都连，增加路径多样性
                    foreach (int targetIndex in possibleTargets)
                    {
                        CreateEdge(srcNode, nextLayer[targetIndex]);
                    }
                }
            }
        }

        // 第二步：智能修补（解决左右隔离，同时保持无交叉）
        for (int j = 0; j < nextLayer.Count; j++)
        {
            string targetId = nextLayer[j];
            bool hasIncoming = false;
            
            foreach (string srcId in currentLayer)
            {
                if (Edges[srcId].Contains(targetId))
                {
                    hasIncoming = true;
                    break;
                }
            }

            if (!hasIncoming)
            {
                // 能连到j号节点的，只有上一层的j-1和j号节点
                int srcIndex;
                if (j - 1 >= 0 && j - 1 < currentLayer.Count)
                {
                    srcIndex = j - 1;
                }
                else if (j < currentLayer.Count)
                {
                    srcIndex = j;
                }
                else
                {
                    // 极端情况：连到上一层最后一个节点
                    srcIndex = currentLayer.Count - 1;
                }

                CreateEdge(currentLayer[srcIndex], targetId);
                GD.Print($"   ⚠️ 智能修补连接: {currentLayer[srcIndex]} -> {targetId}");
            }
        }
    }
    
    GD.Print("✅ 最终版无交叉连接完成！零交叉+全连通+边界修复");
}
        // ---------------------------------------------------------
        // 辅助工具：创建一条边
        // ---------------------------------------------------------
        private void CreateEdge(string fromId, string toId)
        {
            if (!Edges.ContainsKey(fromId)) 
                Edges[fromId] = new List<string>();
            
            if (!Edges[fromId].Contains(toId))
            {
                Edges[fromId].Add(toId);
            }
        }
                // ---------------------------------------------------------
        // 调试工具：打印完整地图拓扑
        // ---------------------------------------------------------
        private void DebugPrintMap()
        {
            GD.Print("\n" + new string('=', 50));
            GD.Print("           🗺️  完整地图拓扑结构");
            GD.Print(new string('=', 50));
            
            for (int i = 0; i < Config.TotalLayers; i++)
            {
                string layerStr = $"[层 {i}] ";
                
                // 遍历该层每个节点
                foreach (string nodeId in _nodesByLayer[i])
                {
                    var node = Nodes[nodeId];
                    // 获取该节点指向哪些节点
                    string connections = string.Join(",", Edges[nodeId]);
                    
                    // 简化打印名字 (太长了占地方)
                    string shortName = node.Type.ToString().Substring(0, Math.Min(3, node.Type.ToString().Length));
                    
                    layerStr += $"{shortName}({nodeId})→[{connections}]  ";
                }
                GD.Print(layerStr);
            }
            GD.Print(new string('=', 50) + "\n");
        }
                // ---------------------------------------------------------
        // 阶段5：计算节点屏幕坐标
        // ---------------------------------------------------------
        private void CalculateNodePositions()
{
    GD.Print("\n📍 开始计算节点坐标 (起点在下，Boss在上)...");

    // 遍历每一层
    for (int layer = 0; layer < Config.TotalLayers; layer++)
    {
        List<string> nodesInThisLayer = _nodesByLayer[layer];
        int nodeCount = nodesInThisLayer.Count;

        // 1. 计算 Y 坐标（核心翻转）
        // Layer 0 (起点) 在最下面 (Y值最大)
        // 最后一层 (Boss) 在最上面 (Y值最小)
        float y = (Config.TotalLayers - 1 - layer) * LayerVerticalSpacing;
        
        // 2. 计算 X 起始坐标（居中）
        float totalWidth = (nodeCount - 1) * NodeHorizontalSpacing;
        float startX = -totalWidth / 2;

        // 3. 赋值坐标
        for (int i = 0; i < nodeCount; i++)
        {
            string nodeId = nodesInThisLayer[i];
            if (Nodes.TryGetValue(nodeId, out MapNode node))
            {
                float x = startX + i * NodeHorizontalSpacing;
                node.Position = new Vector2(x, y);
            }
        }
    }
    
    GD.Print("✅ 坐标计算完毕 (起点在下，Boss在上)");
}
        // ---------------------------------------------------------
        // 辅助工具方法
        // ---------------------------------------------------------
        
        // 辅助工具：创建节点并加入字典
        private MapNode CreateNode(int layer, NodeType type)
        {
            string id = $"node_{_nodeIdCounter++}";
            var node = new MapNode(id, layer, type);
            Nodes.Add(id, node);
            if (!Edges.ContainsKey(id)) Edges[id] = new List<string>();
            return node;
        }

              // 专门用来测试当前步骤的方法
                private void TestFullMapGeneration()
        {
            // 重置所有数据
            Nodes.Clear();
            Edges.Clear();
            _layerNodeCounts.Clear();
            _nodesByLayer.Clear();
            _nodeIdCounter = 0;

            GD.Print("\n=== 开始测试【完整地图生成 + 坐标】 ===");

            // 执行完整流程
            if (!ValidateParameters()) return;
            CreateFixedNodes();
            PlanLayerNodeCounts();
            GenerateNodesByLayer();
            ConnectNodes();
            CalculateNodePositions(); // 🆕 计算坐标

            // 打印带坐标的结果
            DebugPrintMapWithPositions();
        }

        // 新的调试打印：带坐标
        private void DebugPrintMapWithPositions()
        {
            GD.Print("\n" + new string('=', 60));
            GD.Print("           🗺️  地图数据 (含坐标)");
            GD.Print(new string('=', 60));
            
            // 这里为了简洁，我们只打印前几层和后几层验证
            for (int i = 0; i < Config.TotalLayers; i++)
            {
                string layerStr = $"[层 {i}] ";
                foreach (string nodeId in _nodesByLayer[i])
                {
                    var node = Nodes[nodeId];
                    layerStr += $"{node.Type} @ ({node.Position.X:F0}, {node.Position.Y:F0})  ";
                }
                GD.Print(layerStr);
            }
            GD.Print(new string('=', 60));
            GD.Print("💡 提示：坐标已存入 Nodes 字典中每个 node.Position 字段");
        }
    }
}