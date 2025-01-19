namespace Sanet.MekForge.Core.Models.Map;

using Sanet.MekForge.Core.Data;

/// <summary>
/// Represents a segment of a path with movement cost
/// </summary>
public readonly record struct PathSegment(HexPosition From, HexPosition To, int Cost)
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
