using Godot;
using System.Collections.Generic;

namespace YourNamespace
{
    public partial class MapNode : Resource
    {
        [Export] public string Id;           // 唯一ID
        [Export] public int Layer;           // 所在层数 (0为底层)
        [Export] public NodeType Type;       // 节点类型
        [Export] public Vector2 Position;    // 显示坐标 (UI用)
        
        // 构造函数
        public MapNode() { }
        public MapNode(string id, int layer, NodeType type)
        {
            Id = id;
            Layer = layer;
            Type = type;
        }
    }
}