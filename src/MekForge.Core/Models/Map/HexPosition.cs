namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents a position on the hex map, combining coordinates and facing direction
/// </summary>
public readonly record struct HexPosition
{
    public HexCoordinates Coordinates { get; init; }
    public HexDirection Facing { get; init; }

    public HexPosition(HexCoordinates coordinates, HexDirection facing)
    {
        Coordinates = coordinates;
        Facing = facing;
    }

    public HexPosition(int q, int r, HexDirection facing)
        : this(new HexCoordinates(q, r), facing)
    {
    }

    /// <summary>
    /// Calculates the cost in movement points to turn from current facing to target facing
    /// </summary>
    public int GetTurningCost(HexDirection targetFacing)
    {
        var diff = Math.Abs((int)targetFacing - (int)Facing);
        return Math.Min(diff, 6 - diff); // Consider both clockwise and counterclockwise turns
    }

    /// <summary>
    /// Gets a sequence of positions representing the turning steps from current facing to target facing
    /// </summary>
    public IEnumerable<HexPosition> GetTurningSteps(HexDirection targetFacing)
    {
        if (Facing == targetFacing)
            yield break;

        var currentFacingInt = (int)Facing;
        var targetFacingInt = (int)targetFacing;
        
        var clockwiseSteps = (targetFacingInt - currentFacingInt + 6) % 6;
        var counterClockwiseSteps = (currentFacingInt - targetFacingInt + 6) % 6;

        // Choose the shorter turning direction
        if (clockwiseSteps <= counterClockwiseSteps)
        {
            // Turn clockwise
            for (var step = 1; step <= clockwiseSteps; step++)
            {
                var intermediateFacing = (HexDirection)((currentFacingInt + step) % 6);
                yield return new HexPosition(Coordinates, intermediateFacing);
            }
        }
        else
        {
            // Turn counterclockwise
            for (var step = 1; step <= counterClockwiseSteps; step++)
            {
                var intermediateFacing = (HexDirection)((currentFacingInt - step + 6) % 6);
                yield return new HexPosition(Coordinates, intermediateFacing);
            }
        }
    }

    /// <summary>
    /// Gets the required facing direction to move to a neighbor hex
    /// </summary>
    public HexDirection GetRequiredFacingToNeighbor(HexCoordinates neighborCoord)
    {
        return Coordinates.GetDirectionToNeighbour(neighborCoord);
    }
}
