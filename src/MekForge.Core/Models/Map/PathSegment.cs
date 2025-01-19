namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents a segment of a path with movement cost
/// </summary>
public record struct PathSegment(HexPosition From, HexPosition To, int Cost);
