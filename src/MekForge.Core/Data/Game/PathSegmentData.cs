using Sanet.MekForge.Core.Data.Map;

namespace Sanet.MekForge.Core.Data.Game
{
    public record PathSegmentData
    {
        public required HexPositionData From { get; init; }
        public required HexPositionData To { get; init; }
        public required int Cost { get; init; }
    }
}
