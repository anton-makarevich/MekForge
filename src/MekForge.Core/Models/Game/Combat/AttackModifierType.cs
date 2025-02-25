namespace Sanet.MekForge.Core.Models.Game.Combat;

/// <summary>
/// Modifier types that can affect attack rolls
/// </summary>
public enum AttackModifierType
{
    // High level GATOR modifiers
    Gunnery,
    AttackerMovement,
    TargetMovement,
    Range,

    // Detailed modifiers
    Heat,
    ActuatorDamage,
    MultipleTargets,
    TerrainEffect,
    WeaponSpecific,
    SpecialAbility
}
