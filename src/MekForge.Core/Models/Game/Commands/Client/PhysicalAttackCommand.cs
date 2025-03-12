using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record struct PhysicalAttackCommand : IClientCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var command = this;
        var player = game.Players.FirstOrDefault(p => p.Id == command.PlayerId);
        var unit = player?.Units.FirstOrDefault(u => u.Id == command.AttackerUnitId);
        var target = game.Players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == command.TargetUnitId);

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
    public Guid PlayerId { get; init; }
}
