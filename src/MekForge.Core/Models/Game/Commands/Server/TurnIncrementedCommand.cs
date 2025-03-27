using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct TurnIncrementedCommand : IGameCommand
{
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; set; }
    public int TurnNumber { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var localizedTemplate = localizationService.GetString("Command_TurnIncremented");
        return string.Format(localizedTemplate, TurnNumber);
    }
}