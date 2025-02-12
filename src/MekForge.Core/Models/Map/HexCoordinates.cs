using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Exceptions;

namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents coordinates in a hexagonal grid using axial coordinate system
/// </summary>
public readonly record struct HexCoordinates
{
    public const double HexWidth = 100;
    public const double HexHeight = 86.6;
    private const double HexHorizontalSpacing = HexWidth * 0.75;

    /// <summary>
    /// Q coordinate (column)
    /// </summary>
    public int Q { get; init; }
    
    /// <summary>
    /// R coordinate (row)
    /// </summary>
    public int R { get; init; }

    /// <summary>
    /// Gets the cube coordinate S (derived from Q and R)
    /// </summary>
    public int S { get; }

    // Cached cube coordinates
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    
    /// <summary>
    /// Gets the X coordinate in pixels for rendering
    /// </summary>
    public double H => Q * HexHorizontalSpacing;

    /// <summary>
    /// Gets the Y coordinate in pixels for rendering
    /// </summary>
    public double V => R * HexHeight - (Q % 2 == 0 ? 0 : HexHeight * 0.5);

    public HexCoordinates(HexCoordinateData data) : this(data.Q, data.R) { }
    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;

        S = -Q - R;
        
        // Precompute cube coordinates (X, Y, Z)
        X = Q;
        Z = R - (Q + (Q & 1)) / 2; // Handles staggered row adjustment
        Y = -X - Z;
    }

    // Offsets for the six directions, adjusted for even/odd column rows
    private static readonly (int dQ, int dR)[] OddRowDirections =
    [
        (0, -1), // Direction 0: top
        (1, -1), // Direction 1: top-right
        (1, 0),  // Direction 2: bottom-right
        (0, 1),  // Direction 3: bottom
        (-1, 0), // Direction 4: bottom-left
        (-1, -1) // Direction 5: top-left
    ];

    private static readonly (int dQ, int dR)[] EvenRowDirections =
    [
        (0, -1), // Direction 0: top
        (1, 0),  // Direction 1: top-right
        (1, 1),  // Direction 2: bottom-right
        (0, 1),  // Direction 3: bottom
        (-1, 1), // Direction 4: bottom-left
        (-1, 0)  // Direction 5: top-left
    ];

    public HexCoordinates Neighbor(HexDirection direction)
    {
        var directions = (Q % 2 == 0) ? EvenRowDirections : OddRowDirections;
        var (dQ, dR) = directions[(int)direction % 6];
        return new HexCoordinates(Q + dQ, R + dR);
    }
    
    public HexDirection GetDirectionToNeighbour(HexCoordinates neighbour)
    {
        var dQ = neighbour.Q - Q;
        var dR = neighbour.R - R;

        var directions = (Q % 2 == 0) ? EvenRowDirections : OddRowDirections;

        for (var i = 0; i < directions.Length; i++)
        {
            if (directions[i].Item1 == dQ && directions[i].Item2 == dR)
            {
                return (HexDirection)i;
            }
        }

        throw new WrongHexException(neighbour, "Neighbour is not adjacent to center.");
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
        // Use Manhattan distance in cube space
        return Math.Max(Math.Abs(X - other.X), Math.Max(Math.Abs(Y - other.Y), Math.Abs(Z - other.Z)));
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

        for (var i = 0; i <= n; i++)
        {
            var t = 1.0f * i / n;
            var x = (int)Math.Round(X * (1 - t) + target.X * t);
            var z = (int)Math.Round(Z * (1 - t) + target.Z * t);

            // Convert back to axial coordinates
            var q = x;
            var r = z + (x + (x & 1)) / 2;
            result.Add(new HexCoordinates(q, r));
        }

        return result;
    }
    
    public HexCoordinateData ToData() => new(Q, R);

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }

    public override string ToString()
    {
        return $"{Q:D2}{R:D2}";
    }

    public IEnumerable<HexCoordinates> GetHexesInFiringArc(HexDirection facing, FiringArc arc, int range)
    {
        var result = new HashSet<HexCoordinates>();
        
        // Get the cube direction vector for the facing direction
        var facingVector = GetCubeDirectionVector(facing);
        
        foreach (var hex in GetCoordinatesInRange(range))
        {
            if (hex == this) continue; // Skip the center hex
            
            // Get the vector to the target hex in cube coordinates
            var dx = hex.X - this.X;
            var dy = hex.Y - this.Y;
            var dz = hex.Z - this.Z;

            // Check if the hex is in the arc using dot product
            if (IsInArc(dx, dy, dz, facingVector.dx, facingVector.dy, facingVector.dz, arc))
            {
                result.Add(hex);
            }
        }

        return result;
    }

    private (int dx, int dy, int dz) GetCubeDirectionVector(HexDirection dir)
    {
        return dir switch
        {
            HexDirection.Top => (0, 1, -1),         // +y, -z
            HexDirection.TopRight => (1, 0, -1),    // +x, -z
            HexDirection.BottomRight => (1, -1, 0), // +x, -y
            HexDirection.Bottom => (0, -1, 1),      // -y, +z
            HexDirection.BottomLeft => (-1, 0, 1),  // -x, +z
            HexDirection.TopLeft => (-1, 1, 0),     // -x, +y
            _ => throw new ArgumentException("Invalid direction", nameof(dir))
        };
    }

    private bool IsInArc(int dx, int dy, int dz, int fdx, int fdy, int fdz, FiringArc arc)
    {
        // Calculate dot product between the target vector and facing vector
        var dot = dx * fdx + dy * fdy + dz * fdz;
        var targetLength = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        var facingLength = Math.Sqrt(fdx * fdx + fdy * fdy + fdz * fdz);
        
        // Calculate angle in radians
        var cosAngle = dot / (targetLength * facingLength);
        // Handle floating point precision issues
        cosAngle = Math.Max(-1.0, Math.Min(1.0, cosAngle));
        var angle = Math.Acos(cosAngle);
        var degrees = angle * (180 / Math.PI);

        const double epsilon = 0.0001; // Small value to handle floating point comparisons

        // For side arcs, determine if hex is to the left or right of facing direction
        // Using cross product to determine which side the hex is on
        var cross = fdx * dy - dx * fdy;

        return arc switch
        {
            // Forward arc: -60° to +60° inclusive
            FiringArc.Forward => degrees <= 60 + epsilon,
            // Left arc: -60° to -120° exclusive of forward boundary but inclusive of rear boundary
            FiringArc.Left => degrees > 60 + epsilon && degrees <= 120 + epsilon && cross > 0,
            // Right arc: +60° to +120° exclusive of forward boundary but inclusive of rear boundary
            FiringArc.Right => degrees > 60 + epsilon && degrees <= 120 + epsilon && cross < 0,
            // Rear arc: +120° to +180° exclusive 
            FiringArc.Rear => degrees > 120 + epsilon && degrees <= 180 + epsilon,
            _ => throw new ArgumentException("Invalid arc", nameof(arc))
        };
    }
}