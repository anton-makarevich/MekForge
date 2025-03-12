using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record struct ChangePhaseCommand : IGameCommand
{
    public required PhaseNames Phase { get; init; }
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; }

    public string Format(ILocalizationService localizationService, IGame game)
    {
        var localizedTemplate = localizationService.GetString("Command_ChangePhase"); 
        
        return string.Format(localizedTemplate, Phase);
    }
}