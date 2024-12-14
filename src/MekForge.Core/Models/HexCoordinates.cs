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

    // Offsets for the six directions, adjusted for even/odd column rows
    private static readonly (int dQ, int dR)[] OddRowDirections =
    {
        (0, -1), // Direction 0: top
        (1, -1), // Direction 1: top-right
        (1, 0),  // Direction 2: bottom-right
        (0, 1),  // Direction 3: bottom
        (-1, 0), // Direction 4: bottom-left
        (-1, -1) // Direction 5: top-left
    };

    private static readonly (int dQ, int dR)[] EvenRowDirections =
    {
        (0, -1), // Direction 0: top
        (1, 0),  // Direction 1: top-right
        (1, 1),  // Direction 2: bottom-right
        (0, 1),  // Direction 3: bottom
        (-1, 1), // Direction 4: bottom-left
        (-1, 0)  // Direction 5: top-left
    };

    public HexCoordinates Neighbor(int direction)
    {
        var directions = (Q % 2 == 0) ? EvenRowDirections : OddRowDirections;
        var (dQ, dR) = directions[direction % 6];
        return new HexCoordinates(Q + dQ, R + dR);
    }
    
    /// <summary>
    /// Returns adjacent hex coordinates in all six directions
    /// </summary>
    public IEnumerable<HexCoordinates> GetAdjacentCoordinates()
    {
        var directions = (Q % 2 == 0) ? EvenRowDirections : OddRowDirections;
        foreach (var (dQ, dR) in directions)
        {
            yield return new HexCoordinates(Q + dQ, R + dR);
        }
    }

    /// <summary>
    /// Calculates distance to another hex
    /// </summary>
    public int DistanceTo(HexCoordinates other)
    {
        // Convert axial to cube coordinates
        var x1 = Q;
        var z1 = R - (Q + (Q % 2)) / 2; // Fix staggered row handling
        var y1 = -x1 - z1;

        var x2 = other.Q;
        var z2 = other.R - (other.Q + (other.Q % 2)) / 2; // Fix staggered row handling
        var y2 = -x2 - z2;

        // Use Manhattan distance in cube space
        return Math.Max(Math.Abs(x1 - x2), Math.Max(Math.Abs(y1 - y2), Math.Abs(z1 - z2)));
    }

    /// <summary>
    /// Returns all hex coordinates within the specified range (inclusive)
    /// Requires optimisation to remove DistanceTo
    /// </summary>
    public IEnumerable<HexCoordinates> GetCoordinatesInRange(int range)
    {
        for (var dQ = -range; dQ <= range; dQ++)
        {
            for (var dR = -range; dR <= range; dR++)
            {
                var candidate = new HexCoordinates(Q + dQ, R + dR);
                if (DistanceTo(candidate) <= range)
                {
                    yield return candidate;
                }
            }
        }
    }
    
    /// <summary>
    /// Gets coordinates of hexes that form a line between two points
    /// </summary>
    public List<HexCoordinates> LineTo(HexCoordinates target)
    {
        if (Equals(target))
        {
            return [target];
        }
        var n = DistanceTo(target);
        var result = new List<HexCoordinates>();

        // Convert to cube coordinates for linear interpolation
        var x1 = Q;
        var z1 = R - (Q + (Q & 1)) / 2;
        var y1 = -x1 - z1;

        var x2 = target.Q;
        var z2 = target.R - (target.Q + (target.Q & 1)) / 2;
        var y2 = -x2 - z2;

        for (int i = 0; i <= n; i++)
        {
            var t = 1.0f * i / n;
            var x = (int)Math.Round(x1 * (1 - t) + x2 * t);
            var y = (int)Math.Round(y1 * (1 - t) + y2 * t);
            var z = (int)Math.Round(z1 * (1 - t) + z2 * t);

            // Convert back to axial coordinates
            var q = x;
            var r = z + (x + (x & 1)) / 2;
            result.Add(new HexCoordinates(q, r));
        }

        return result;
    }
}