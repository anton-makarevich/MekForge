using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record DeployUnitCommand : ClientCommand
{
    public required Guid UnitId { get; init; }
    public required HexCoordinateData Position { get; init; }
    public required int Direction { get; init; }

    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player == null) return string.Empty;

        var unit = player.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit == null) return string.Empty;

        var position = new HexCoordinates(Position);
        var facingHex = position.Neighbor((HexDirection)Direction);

        var localizedTemplate = localizationService.GetString("Command_DeployUnit"); 
        
        return string.Format(localizedTemplate,
            player.Name,
            unit.Model,
            position.ToString(),
            facingHex.ToString());
    }
}