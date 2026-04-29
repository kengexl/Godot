using Godot;
using System.Collections.Generic;

namespace YourNamespace
{
    public partial class MapRenderer : Node2D
    {
        [Export] public MapGenerator MapGenerator; // 引用地图生成器
        [Export] public PackedScene MapNodePrefab; // 刚才创建的节点预制体
        [Export] public Node2D LinesContainer; // 存放连线的容器
        [Export] public Node2D NodesContainer; // 存放节点的容器

        [Export] public Color LineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 连线颜色
        [Export] public float LineWidth = 3f; // 连线宽度

        // 缓存生成的实例，方便清除
        private List<Line2D> _lines = new();
        private List<Button> _nodeInstances = new();

        public override void _Ready()
        {
            // 监听地图生成完成信号
            if (MapGenerator != null)
            {
                MapGenerator.OnMapGenerated += RenderMap;
            }
        }

        // 渲染完整地图
        public void RenderMap()
        {
            ClearMap(); // 先清除旧地图

            if (MapGenerator.Nodes.Count == 0)
            {
                GD.PrintErr("没有地图数据可以渲染！");
                return;
            }

            GD.Print("\n🎨 开始渲染地图...");
            DrawAllLines();
            DrawAllNodes();
            GD.Print("✅ 地图渲染完成！");
        }

        // 1. 先画所有连线（在底层）
        private void DrawAllLines()
        {
            foreach (var edge in MapGenerator.Edges)
            {
                string fromId = edge.Key;
                foreach (string toId in edge.Value)
                {
                    DrawLine(fromId, toId);
                }
            }
        }

        // 画一条单独的连线
        private void DrawLine(string fromId, string toId)
        {
            if (!MapGenerator.Nodes.TryGetValue(fromId, out MapNode fromNode) ||
                !MapGenerator.Nodes.TryGetValue(toId, out MapNode toNode))
            {
                return;
            }

            Line2D line = new Line2D();
            line.DefaultColor = LineColor;
            line.Width = LineWidth;
            line.Points = new Vector2[] { fromNode.Position, toNode.Position };
            line.ZIndex = -1; // 确保连线在节点下面

            LinesContainer.AddChild(line);
            _lines.Add(line);
        }

        // 2. 再画所有节点（在上层）
        private void DrawAllNodes()
        {
            foreach (var nodeData in MapGenerator.Nodes.Values)
            {
                DrawNode(nodeData);
            }
        }

        // 画一个单独的节点
        private void DrawNode(MapNode nodeData)
        {
            if (MapNodePrefab == null) return;

            Button nodeInstance = MapNodePrefab.Instantiate<Button>();
            nodeInstance.Position = nodeData.Position;
            nodeInstance.Name = nodeData.Id;

            // 获取Label并设置文字
            Label label = nodeInstance.GetNode<Label>("Label_Type");
            if (label != null)
            {
                label.Text = GetNodeTypeShortName(nodeData.Type);
            }

            // 根据节点类型设置不同颜色
            SetNodeColor(nodeInstance, nodeData.Type);

            // 绑定点击事件
            nodeInstance.Pressed += () => OnNodeClicked(nodeData);

            NodesContainer.AddChild(nodeInstance);
            _nodeInstances.Add(nodeInstance);
        }

        // 节点点击事件
        private void OnNodeClicked(MapNode nodeData)
        {
            GD.Print($"🖱️ 点击了节点: {nodeData.Type} ({nodeData.Id})");
            // 这里可以写进入战斗、打开宝箱、休息等逻辑
            // 例如: if (nodeData.Type == NodeType.Enemy) GetTree().ChangeSceneToFile("res://Scenes/Battle.tscn");
        }

        // 清除当前地图
        public void ClearMap()
        {
            foreach (var line in _lines) line.QueueFree();
            foreach (var node in _nodeInstances) node.QueueFree();
            _lines.Clear();
            _nodeInstances.Clear();
        }

        #region 辅助工具方法
        // 获取节点类型的短名称
        private string GetNodeTypeShortName(NodeType type)
        {
            return type switch
            {
                NodeType.Start => "起点",
                NodeType.Boss => "BOSS",
                NodeType.Elite => "精英",
                NodeType.Treasure => "宝箱",
                NodeType.Rest => "篝火",
                NodeType.Enemy => "小怪",
                NodeType.Event => "事件",
                NodeType.Shop => "商店",
                _ => "未知"
            };
        }

        // 根据类型设置节点颜色
        // 根据类型设置节点颜色（Godot 4 兼容版）
private void SetNodeColor(Button button, NodeType type)
{
    // 创建一个新的StyleBoxFlat
    StyleBoxFlat styleBox = new StyleBoxFlat();
    
    // 根据类型设置背景色
    styleBox.BgColor = type switch
    {
        NodeType.Start => new Color(0.2f, 0.8f, 0.2f), // 绿色
        NodeType.Boss => new Color(0.9f, 0.2f, 0.2f), // 红色
        NodeType.Elite => new Color(0.7f, 0.2f, 0.8f), // 紫色
        NodeType.Treasure => new Color(0.9f, 0.8f, 0.2f), // 金色
        NodeType.Rest => new Color(0.2f, 0.6f, 0.9f), // 蓝色
        NodeType.Enemy => new Color(0.8f, 0.5f, 0.2f), // 橙色
        NodeType.Event => new Color(0.5f, 0.5f, 0.5f), // 灰色
        NodeType.Shop => new Color(0.2f, 0.8f, 0.8f), // 青色
        _ => new Color(0.5f, 0.5f, 0.5f)
    };

    

    // 覆盖按钮的normal状态样式
    button.AddThemeStyleboxOverride("normal", styleBox);
    
    // 同时设置hover和pressed状态的颜色（稍微暗一点）
    StyleBoxFlat hoverStyle = (StyleBoxFlat)styleBox.Duplicate();
    hoverStyle.BgColor = styleBox.BgColor * 0.8f;
    button.AddThemeStyleboxOverride("hover", hoverStyle);

    StyleBoxFlat pressedStyle = (StyleBoxFlat)styleBox.Duplicate();
    pressedStyle.BgColor = styleBox.BgColor * 0.6f;
    button.AddThemeStyleboxOverride("pressed", pressedStyle);
}
        #endregion
    }
}