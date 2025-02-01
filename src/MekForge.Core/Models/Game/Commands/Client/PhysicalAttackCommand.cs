using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record PhysicalAttackCommand : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var unit = player?.Units.FirstOrDefault(u => u.Id == AttackerUnitId);
        var target = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == TargetUnitId);

        if (unit == null || target == null) return string.Empty;

        var localizedTemplate = localizationService.GetString("Command_PhysicalAttack");
        return string.Format(localizedTemplate, 
            player?.Name, 
            unit.Name,
            target.Name,
            AttackType); 
    }

    public required Guid AttackerUnitId { get; init; }
    public required Guid TargetUnitId { get; init; }
    public required PhysicalAttackType AttackType { get; init; }
}
