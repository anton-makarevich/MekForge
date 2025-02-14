namespace Sanet.MekForge.Core.Models.Map.Terrains;

public class LightWoodsTerrain : Terrain
{
    public override string Id => "LightWoods";
    public override int Height => 2;
    public override int InterveningFactor => 1;
    public override int MovementCost => 2;
}
