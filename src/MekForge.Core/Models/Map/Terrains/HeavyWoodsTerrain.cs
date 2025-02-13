namespace Sanet.MekForge.Core.Models.Map.Terrains;

public class HeavyWoodsTerrain : Terrain
{
    public override string Id => "HeavyWoods";
    public override int Height => 2;
    public override int InterveningFactor => 2;
    public override int MovementCost => 3;
}
