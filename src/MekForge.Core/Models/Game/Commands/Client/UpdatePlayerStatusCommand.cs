using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record UpdatePlayerStatusCommand: ClientCommand
{
    public required PlayerStatus PlayerStatus { get; init; }
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var localizedTemplate = localizationService.GetString("Command_UpdatePlayerStatus"); 
        return string.Format(localizedTemplate, player.Name, PlayerStatus);
    }
}