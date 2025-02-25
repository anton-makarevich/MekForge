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
    public required AttackModifier GunneryBase { get; init; }

    /// <summary>
    /// Modifier based on attacker's movement type
    /// </summary>
    public required AttackModifier AttackerMovement { get; init; }

    /// <summary>
    /// Modifier based on target's movement distance
    /// </summary>
    public required AttackModifier TargetMovement { get; init; }

    /// <summary>
    /// List of other modifiers with descriptions
    /// </summary>
    public required IReadOnlyList<AttackModifier> OtherModifiers { get; init; }

    /// <summary>
    /// Modifier based on weapon range to target
    /// </summary>
    public required AttackModifier RangeModifier { get; init; }

    /// <summary>
    /// List of terrain modifiers along the line of sight
    /// </summary>
    public required IReadOnlyList<AttackModifier> TerrainModifiers { get; init; }

    /// <summary>
    /// Whether there is a clear line of sight to the target
    /// </summary>
    public required bool HasLineOfSight { get; init; }

    /// <summary>
    /// All modifiers combined into a single list
    /// </summary>
    public IReadOnlyList<AttackModifier> AllModifiers => new[]
    {
        GunneryBase,
        AttackerMovement,
        TargetMovement,
        RangeModifier
    }.Concat(OtherModifiers)
     .Concat(TerrainModifiers)
     .ToList();

    /// <summary>
    /// Total modifier for the attack
    /// </summary>
    public int Total => HasLineOfSight ? 
        AllModifiers.Sum(m => m.Value)
        : int.MaxValue; // Cannot hit if no line of sight
}
