using Sanet.MekForge.Core.Data.Units;

namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Serializable data about a weapon and its target
/// </summary>
public record WeaponTargetData
{
    public required WeaponData Weapon { get; init; }
    public required Guid TargetId { get; init; }
    public required bool IsPrimaryTarget { get; init; }
}
