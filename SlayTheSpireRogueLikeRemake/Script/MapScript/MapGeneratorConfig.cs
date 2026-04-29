using Godot;

namespace YourNamespace
{
    [GlobalClass]
    public partial class MapGeneratorConfig : Resource
    {
        [Export] public int TotalLayers = 10;
        [Export] public int MinNodesPerLayer = 2;
        [Export] public int MaxNodesPerLayer = 4;
        [Export] public int TargetTotalNodes = 25;

        [Export] public int EliteCount = 3;
        [Export] public int TreasureCount = 2;
        [Export] public int RestCount = 2;

        [Export] public bool ForceRestBeforeBoss = true;
        [Export] public int TreasureStartLayer = 3;
        [Export] public float RestNearEliteBonus = 0.5f;
    }
}