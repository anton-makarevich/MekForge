using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record DiceRolledCommand : GameCommand
{
    public required Guid PlayerId { get; init; }
    public required int Roll { get; init; }
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var localizedTemplate = localizationService.GetString("Command_RollDice");
        return string.Format(localizedTemplate, player.Name, Roll);
    }
}
