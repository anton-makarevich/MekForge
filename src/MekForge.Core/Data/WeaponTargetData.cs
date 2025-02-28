namespace Sanet.MekForge.Core.Data;

/// <summary>
/// Serializable data about a weapon and its target
/// </summary>
public record WeaponTargetData
{
    public required WeaponData Weapon { get; init; }
    public required Guid TargetId { get; init; }
    public required bool IsPrimaryTarget { get; init; }
}
