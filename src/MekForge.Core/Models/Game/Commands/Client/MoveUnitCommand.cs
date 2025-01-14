using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record MoveUnitCommand: ClientCommand
{

    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;
        var unit = player.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit == null) return string.Empty;
        var destination = new HexCoordinates(Destination);
        var localizedTemplate = localizationService.GetString("Command_MoveUnit");
        return string.Format(localizedTemplate, player.Name, unit.Name, destination, MovementType); 
    }

    public required Guid UnitId { get; init; }
    public required HexCoordinateData Destination { get; init; }
    public required MovementType MovementType { get; init; }
    public required int Direction { get; init; }
    public required int MovementPoints { get; init; }
}