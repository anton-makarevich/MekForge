using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record MoveUnitCommand(
    Guid UnitId,
    HexCoordinateData Destination) : ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var unit = player.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit == null) return string.Empty;
        var destination = new HexCoordinates(Destination);
        var localizedTemplate = localizationService.GetString("Command_MoveUnit");
        return string.Format(localizedTemplate, player.Name, unit.Name, destination); 
    }
}