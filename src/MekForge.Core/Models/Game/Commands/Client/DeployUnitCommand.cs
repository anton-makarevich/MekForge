using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record DeployUnitCommand : ClientCommand
{
    public required Guid UnitId { get; init; }
    public required HexCoordinateData Position { get; init; }
    public required int Direction { get; init; }
}