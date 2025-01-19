namespace Sanet.MekForge.Core.Models.Map;

using Sanet.MekForge.Core.Data;

/// <summary>
/// Represents a segment of a path with movement cost
/// </summary>
public record struct PathSegment
{
    public HexPosition From { get; }
    public HexPosition To { get; }
    public int Cost { get; }

    public PathSegment(HexPosition from, HexPosition to, int cost)
    {
        From = from;
        To = to;
        Cost = cost;
    }

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
