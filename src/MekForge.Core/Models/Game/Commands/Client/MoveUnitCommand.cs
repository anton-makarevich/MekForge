using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record MoveUnitCommand: ClientCommand
{
    public override string Format(ILocalizationService localizationService, IGame game)
    {
        var player = game.Players.FirstOrDefault(p => p.Id == PlayerId);
        var unit = player?.Units.FirstOrDefault(u => u.Id == UnitId);
        if (unit is not { Position: not null }) return string.Empty;
        var localizedTemplate = localizationService.GetString("Command_MoveUnit");
        var position = MovementPath.Count>0 ? 
            MovementPath.Last().To
            : unit.Position.ToData();
        var facingHex = new HexCoordinates(position.Coordinates).Neighbor((HexDirection)position.Facing);
        return string.Format(localizedTemplate, 
            player?.Name, 
            unit.Name,
            new HexCoordinates(position.Coordinates),
            facingHex,
            MovementType); 
    }

    public required Guid UnitId { get; init; }
    public required MovementType MovementType { get; init; }
    public required List<PathSegmentData> MovementPath { get; init; }
}