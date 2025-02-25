using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Models.Game.Combat;

/// <summary>
/// Detailed breakdown of GATOR modifiers affecting an attack
/// </summary>
public record ToHitBreakdown
{
    /// <summary>
    /// Base gunnery skill of the attacker
    /// </summary>
    public required int GunneryBase { get; init; }

    /// <summary>
    /// Modifier based on attacker's movement type
    /// </summary>
    public required int AttackerMovement { get; init; }

    /// <summary>
    /// Modifier based on target's movement distance
    /// </summary>
    public required int TargetMovement { get; init; }

    /// <summary>
    /// List of other modifiers with descriptions
    /// </summary>
    public required IReadOnlyList<(string Reason, int Modifier)> OtherModifiers { get; init; }

    /// <summary>
    /// Modifier based on weapon range to target
    /// </summary>
    public required int RangeModifier { get; init; }

    /// <summary>
    /// List of terrain modifiers along the line of sight
    /// </summary>
    public required IReadOnlyList<(Hex Hex, int Modifier)> TerrainModifiers { get; init; }

    /// <summary>
    /// Whether there is a clear line of sight to the target
    /// </summary>
    public required bool HasLineOfSight { get; init; }

    /// <summary>
    /// Total modifier for the attack
    /// </summary>
    public int Total => HasLineOfSight ? 
        GunneryBase + 
        AttackerMovement + 
        TargetMovement + 
        OtherModifiers.Sum(m => m.Modifier) + 
        RangeModifier +
        TerrainModifiers.Sum(t => t.Modifier)
        : int.MaxValue; // Cannot hit if no line of sight
}
