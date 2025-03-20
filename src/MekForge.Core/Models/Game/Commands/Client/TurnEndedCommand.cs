using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record struct TurnEndedCommand : IClientCommand
{
    public Guid GameOriginId { get; set; }
    public Guid PlayerId { get; init; }
    public DateTime Timestamp { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var playerId = PlayerId;
        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        var localizedTemplate = localizationService.GetString("Command_TurnEnded");
        return string.Format(localizedTemplate, player?.Name);
    }
}
