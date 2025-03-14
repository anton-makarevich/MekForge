namespace Sanet.MekForge.Core.Data.Game;

using Sanet.MekForge.Core.Models.Game.Dice;

/// <summary>
/// Represents all hit locations from a weapon attack
/// </summary>
public record AttackHitLocationsData(
    List<HitLocationData> HitLocations,
    int TotalDamage,
    List<DiceResult> ClusterRoll,
    int MissilesHit);