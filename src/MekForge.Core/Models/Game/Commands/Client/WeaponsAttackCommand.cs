using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record WeaponsAttackCommand : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var unit = player?.Units.FirstOrDefault(u => u.Id == AttackerUnitId);
        var target = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == TargetUnitId);

        if (unit == null || target == null) return string.Empty;

        var localizedTemplate = localizationService.GetString("Command_WeaponsAttack");
        return string.Format(localizedTemplate, 
            player?.Name, 
            unit.Name,
            target.Name,
            WeaponGroupIndex); 
    }

    public required Guid AttackerUnitId { get; init; }
    public required Guid TargetUnitId { get; init; }
    public required int WeaponGroupIndex { get; init; }
}
