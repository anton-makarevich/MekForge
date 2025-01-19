namespace Sanet.MekForge.Core.Data
{
    public record struct PathSegmentData
    {
        public required HexPositionData From { get; init; }
        public required HexPositionData To { get; init; }
        public required int Cost { get; init; }
    }
}
