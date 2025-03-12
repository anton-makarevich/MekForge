using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record JoinGameCommand: ClientCommand
{
    public required string PlayerName { get; init; }
    public required List<UnitData> Units { get; init; }
    public required string Tint { get; init; }
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var localizedTemplate = localizationService.GetString("Command_JoinGame"); 
        return string.Format(localizedTemplate, PlayerName, Units.Count);
    }
}