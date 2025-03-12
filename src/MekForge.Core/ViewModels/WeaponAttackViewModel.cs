using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.ViewModels;

public record WeaponAttackViewModel
{
    public required HexCoordinates From { get; init; }
    public required HexCoordinates To { get; init; }
    public required Weapon Weapon { get; init; }
    public required string AttackerTint { get; init; } 
    public required int LineOffset { get; init; }
}
