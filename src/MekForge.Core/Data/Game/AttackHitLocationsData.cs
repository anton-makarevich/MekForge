namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Represents all hit locations from a weapon attack
/// </summary>
public record AttackHitLocationsData(
    List<HitLocationData> HitLocations,
    int TotalDamage,
    int ClusterRollResult,
    int MissilesHit);