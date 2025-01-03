using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record ChangePhaseCommand : GameCommand
{
    public PhaseNames Phase { get; init; }
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var localizedTemplate = localizationService.GetString("Command_ChangePhase"); 
        
        return string.Format(localizedTemplate, Phase);
    }
}