namespace Sanet.MekForge.Core.Data
{
    public record HexData
    {
        public required HexCoordinateData Coordinates { get; init; }
        public required string[] TerrainTypes { get; init; }
        public int Level { get; init; } = 0;
    }
}
