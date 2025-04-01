using Sanet.MakaMek.Core.Data.Game;

namespace Sanet.MakaMek.Core.Models.Map;

/// <summary>
/// Represents a segment of a path with movement cost
/// </summary>
public record PathSegment(HexPosition From, HexPosition To, int Cost)
{
    public PathSegment(PathSegmentData data)
        : this(new HexPosition(data.From), new HexPosition(data.To), data.Cost)
    {
    }

    public PathSegmentData ToData() => new()
    {
        From = From.ToData(),
        To = To.ToData(),
        Cost = Cost
    };
};
