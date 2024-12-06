namespace Sanet.MekForge.Core.Models;

/// <summary>
/// Represents coordinates in a hexagonal grid using axial coordinate system
/// </summary>
public readonly record struct HexCoordinates
{
    public const double HexWidth = 100;
    public const double HexHeight = 86.6;
    public const double HexHorizontalSpacing = HexWidth * 0.75;

    /// <summary>
    /// Q coordinate (column)
    /// </summary>
    public int Q { get; init; }
    
    /// <summary>
    /// R coordinate (row)
    /// </summary>
    public int R { get; init; }
    
    /// <summary>
    /// Gets the X coordinate in pixels for rendering
    /// </summary>
    public double X => Q * HexHorizontalSpacing;

    /// <summary>
    /// Gets the Y coordinate in pixels for rendering
    /// </summary>
    public double Y => R * HexHeight + (Q % 2 == 0 ? 0 : HexHeight * 0.5);

    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    /// <summary>
    /// Gets the cube coordinate S (derived from Q and R)
    /// </summary>
    public int S => -Q - R;

    /// <summary>
    /// Returns adjacent hex coordinates in all six directions
    /// </summary>
    public IEnumerable<HexCoordinates> GetAdjacentCoordinates()
    {
        yield return new HexCoordinates { Q = Q + 1, R = R };     // East
        yield return new HexCoordinates { Q = Q + 1, R = R - 1 }; // Northeast
        yield return new HexCoordinates { Q = Q, R = R - 1 };     // Northwest
        yield return new HexCoordinates { Q = Q - 1, R = R };     // West
        yield return new HexCoordinates { Q = Q - 1, R = R + 1 }; // Southwest
        yield return new HexCoordinates { Q = Q, R = R + 1 };     // Southeast
    }

    /// <summary>
    /// Calculates distance to another hex
    /// </summary>
    public int DistanceTo(HexCoordinates other)
    {
        var dQ = Math.Abs(Q - other.Q);
        var dR = Math.Abs(R - other.R);
        var dS = Math.Abs(S - other.S);
        return Math.Max(Math.Max(dQ, dR), dS);
    }

    /// <summary>
    /// Returns all hex coordinates within the specified range (inclusive)
    /// </summary>
    public IEnumerable<HexCoordinates> GetCoordinatesInRange(int range)
    {
        for (var q = -range; q <= range; q++)
        {
            var r1 = Math.Max(-range, -q - range);
            var r2 = Math.Min(range, -q + range);
            
            for (var r = r1; r <= r2; r++)
            {
                yield return new HexCoordinates(Q + q, R + r);
            }
        }
    }
}