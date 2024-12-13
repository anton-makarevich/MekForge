using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models;

public class BattleState
{
    private readonly BattleMap _map;
    private readonly Dictionary<HexCoordinates, Unit> _deployedUnits = new();

    public BattleState(BattleMap map)
    {
        _map = map;
    }

    public bool TryDeployUnit(Unit unit, HexCoordinates coordinates)
    {
        if (_deployedUnits.ContainsKey(coordinates)) return false;
        var hex = _map.GetHex(coordinates);
        if (hex == null) return false;

        unit.Deploy(coordinates);
        _deployedUnits[coordinates] = unit;

        return true;
    }

    public bool TryMoveUnit(Unit unit, HexCoordinates destination)
    {
        if (!unit.Position.HasValue || !_deployedUnits.ContainsValue(unit)) return false;
        if (_deployedUnits.ContainsKey(destination)) return false;

        var oldPosition = unit.Position.Value;
        _deployedUnits.Remove(oldPosition);
        unit.MoveTo(destination);
        _deployedUnits[destination] = unit;

        return true;
    }

    public bool HasLineOfSight(Unit attacker, Unit target)
    {
        if (!attacker.Position.HasValue || !target.Position.HasValue) return false;

        var start = attacker.Position.Value;
        var end = target.Position.Value;

        _map.HasLineOfSight(start,end);
        var lineOfSight = BattleMap.GetLineOfSight(start, end);

        foreach (var coordinates in lineOfSight)
        {
            if (coordinates == start || coordinates == end) continue;
            if (_deployedUnits.ContainsKey(coordinates)) return false; //Should be more complicated than boolean as can also be partial
        }

        return true;
    }

    public List<HexCoordinates>? FindPath(Unit unit, HexCoordinates target)
    {
        if (!unit.Position.HasValue) return null;

        return _map.FindPath(
            unit.Position.Value,
            target,
            unit.GetMovementPoints(MovementType.Walk));
    }
}
