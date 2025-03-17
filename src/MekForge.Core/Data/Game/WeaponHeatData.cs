namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Heat generated from firing a weapon
/// </summary>
public record struct WeaponHeatData
{
    public required string WeaponName { get; init; }
    public required int HeatPoints { get; init; }
}
