using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record RollDiceCommand : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var localizedTemplate = localizationService.GetString("Command_RollDice");

        return string.Format(localizedTemplate, player.Name);
    }
}
