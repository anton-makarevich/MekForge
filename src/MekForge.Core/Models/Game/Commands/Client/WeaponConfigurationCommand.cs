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

        var unit = player?.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit == null) return string.Empty;

        return Configuration.Type switch
        {
            WeaponConfigurationType.TorsoRotation => $"{player.Name}'s {unit.Name} rotates torso to {(HexDirection)Configuration.Value}",
            WeaponConfigurationType.ArmsFlip => $"{player.Name}'s {unit.Name} flips arms to {(Configuration.Value == 1 ? "forward" : "backward")}",
            _ => string.Empty
        };
    }
}
