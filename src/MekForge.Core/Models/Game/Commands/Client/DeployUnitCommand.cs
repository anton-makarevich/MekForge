using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record DeployUnitCommand : ClientCommand
{
    public Guid UnitId { get; init; }
    public HexCoordinateData Position { get; init; }
}