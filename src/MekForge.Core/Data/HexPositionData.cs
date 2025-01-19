namespace Sanet.MekForge.Core.Data
{
    public record struct HexPositionData
    {
        public required HexCoordinateData Coordinates { get; init; }
        public required int Facing { get; init; }
    }
}
