using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record struct TurnEndedCommand : IClientCommand
{
    public Guid GameOriginId { get; set; }
    public Guid PlayerId { get; init; }
    public DateTime Timestamp { get; init; } 

    public string Format(ILocalizationService localizationService, IGame game)
    {
        return localizationService.GetString("Command_TurnEnded");
    }
}
