using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;

public class MoveUnitCommandBuilder
{
    private Guid? _unitId;
    private readonly Guid _gameId;
    private MovementType? _movementType;
    private HexCoordinates? _destination;
    private HexDirection? _direction;
    private readonly Guid _playerId;

    public MoveUnitCommandBuilder(Guid gameId,  Guid playerId)
    {
        _gameId = gameId;
        _playerId = playerId;
    }

    public void SetUnit(Unit unit)
    {
        _unitId = unit.Id;
    }

    public void SetMovementType(MovementType movementType)
    {
        _movementType = movementType;
    }

    public void SetDestination(HexCoordinates coordinates)
    {
        _destination = coordinates;
    }

    public void SetDirection(HexDirection direction)
    {
        _direction = direction;
    }

    public MoveUnitCommand? Build()
    {
        if (_unitId == null || _movementType == null || _destination == null || _direction == null)
            return null;

        return new MoveUnitCommand
        {
            GameOriginId = _gameId,
            UnitId = _unitId.Value,
            MovementType = _movementType.Value,
            Destination = _destination.Value.ToData(),
            Direction = (int)_direction.Value,
            PlayerId = _playerId
        };
    }

    public void Reset()
    {
        _unitId = null;
        _movementType = null;
        _destination = null;
        _direction = null;
    }
}
