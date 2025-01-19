namespace Sanet.MekForge.Core.Models.Map;

using Sanet.MekForge.Core.Data;

/// <summary>
/// Represents a segment of a path with movement cost
/// </summary>
public record struct PathSegment(HexPosition From, HexPosition To, int Cost)
{
    public PathSegmentData ToData() => new()
    {
        From = From.ToData(),
        To = To.ToData(),
        Cost = Cost
    };
};
