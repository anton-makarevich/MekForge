namespace Sanet.MekForge.Core.Models.Terrains;

public class ClearTerrain : Terrain
{
    public override string Id => "Clear";
    public override int Height => 0;
    public override int TerrainFactor => 1;
}