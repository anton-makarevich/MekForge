using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record WeaponConfigurationCommand : ClientCommand
{
    public required Guid UnitId { get; set; }
    public required WeaponConfiguration Configuration { get; set; }

    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;

        var unit = player.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit == null || !unit.IsDeployed) return string.Empty;

        return Configuration.Type switch
        {
            WeaponConfigurationType.TorsoRotation => string.Format(
                localizationService.GetString("Command_WeaponConfiguration_TorsoRotation"),
                player.Name,
                unit.Name,
                unit.Position!.Value.Coordinates.Neighbor((HexDirection)Configuration.Value)),
            WeaponConfigurationType.ArmsFlip => string.Format(
                localizationService.GetString("Command_WeaponConfiguration_ArmsFlip"),
                player.Name,
                unit.Name,
                Configuration.Value == 1 
                    ? localizationService.GetString("Direction_Forward")
                    : localizationService.GetString("Direction_Backward")),
            _ => string.Empty
        };
    }
}
