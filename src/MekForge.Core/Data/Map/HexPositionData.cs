namespace Sanet.MekForge.Core.Data.Map
{
    public record HexPositionData
    {
        public required HexCoordinateData Coordinates { get; init; }
        public required int Facing { get; init; }
    }
}
