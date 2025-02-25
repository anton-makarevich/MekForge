namespace Sanet.MekForge.Core.Models.Game.Combat;

/// <summary>
/// Represents a single modifier affecting an attack roll
/// </summary>
public readonly struct AttackModifier
{
    public AttackModifierType Type { get; init; }
    public int Value { get; init; }

    public AttackModifier(AttackModifierType type, int value)
    {
        Type = type;
        Value = value;
    }
}
