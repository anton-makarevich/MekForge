using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record WeaponConfigurationCommand : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var unit = player?.Units.FirstOrDefault(u => u.Id == UnitId);

        if (unit == null) return string.Empty;

        var localizedTemplate = localizationService.GetString("Command_WeaponConfiguration");
        return string.Format(localizedTemplate, 
            player?.Name, 
            unit.Name,
            TurretRotation); 
    }

    public required Guid UnitId { get; init; }
    public required int TurretRotation { get; init; }
}
