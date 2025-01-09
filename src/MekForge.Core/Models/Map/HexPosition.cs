using Sanet.MekForge.Core.Data;

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
    /// Returns a new HexPosition with the same coordinates but different facing
    /// </summary>
    public HexPosition WithFacing(HexDirection newFacing) => this with { Facing = newFacing };

    /// <summary>
    /// Returns a new HexPosition with the same facing but different coordinates
    /// </summary>
    public HexPosition WithCoordinates(HexCoordinates newCoordinates) => this with { Coordinates = newCoordinates };
}
