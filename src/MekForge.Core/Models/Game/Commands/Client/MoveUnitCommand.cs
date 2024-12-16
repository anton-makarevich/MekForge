using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record MoveUnitCommand(
    Guid UnitId,
    HexCoordinateData Destination) : ClientCommand;