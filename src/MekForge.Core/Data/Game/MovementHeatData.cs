using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Heat generated from movement
/// </summary>
public record struct MovementHeatData
{
    public required MovementType MovementType { get; init; }
    public required int MovementPointsSpent { get; init; }
    public required int HeatPoints { get; init; }
}
