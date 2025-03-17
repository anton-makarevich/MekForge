using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Comprehensive heat data for a unit, including sources, dissipation, and totals
/// </summary>
public record struct HeatData
{
    /// <summary>
    /// Heat generated from movement
    /// </summary>
    public required List<MovementHeatData> MovementHeatSources { get; init; }
    
    /// <summary>
    /// Heat generated from weapons
    /// </summary>
    public required List<WeaponHeatData> WeaponHeatSources { get; init; }
    
    /// <summary>
    /// Heat dissipation information
    /// </summary>
    public required HeatDissipationData DissipationData { get; init; }
    
    /// <summary>
    /// Total heat to apply to the unit
    /// </summary>
    public int TotalHeatPoints => 
        MovementHeatSources.Sum(source => source.HeatPoints) + 
        WeaponHeatSources.Sum(source => source.HeatPoints);
    
    /// <summary>
    /// Total heat to dissipate
    /// </summary>
    public int TotalHeatDissipationPoints => DissipationData.DissipationPoints;
}
