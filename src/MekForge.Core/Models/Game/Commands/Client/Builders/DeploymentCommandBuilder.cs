using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;

public class DeploymentCommandBuilder : ClientCommandBuilder
{
    private Guid? _unitId;
    private HexCoordinates? _position;
    private HexDirection? _direction;

    public DeploymentCommandBuilder(Guid gameId, Guid playerId) 
        : base(gameId, playerId)
    {
    }

    public override bool CanBuild => _unitId.HasValue && _position.HasValue && _direction.HasValue;
    
    public void SetUnit(Unit unit) => _unitId = unit.Id;
    public void SetPosition(HexCoordinates pos) => _position = pos;
    public void SetDirection(HexDirection dir) => _direction = dir;
    
    public void Reset()
    {
        _unitId = null;
        _position = null;
        _direction = null;
    }
    
    public override DeployUnitCommand? Build()
    {
        if (_unitId == null || _position == null || _direction == null)
            return null;

        return new DeployUnitCommand
        {
            GameOriginId = GameId,
            PlayerId = PlayerId,
            UnitId = _unitId.Value,
            Position = _position.Value.ToData(),
            Direction = (int)_direction.Value
        };
    }
}
