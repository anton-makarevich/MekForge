using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Models;

public class BattleState
{
    private readonly BattleMap _map;
    private readonly List<Unit> _deployedUnits = [];

    public BattleState(BattleMap map)
    {
        _map = map;
    }

    public bool TryDeployUnit(Unit unit, HexCoordinates coordinates)
    {
        if (!IsHexFree(coordinates)) return false;
        var hex = _map.GetHex(coordinates);
        if (hex == null) return false;

        unit.Deploy(coordinates);
        _deployedUnits.Add(unit);

        return true;
    }

    public bool TryMoveUnit(Unit unit, HexCoordinates destination)
    {
        if (!unit.Position.HasValue) return false;
        if (!IsHexFree(destination)) return false;
        
        unit.MoveTo(destination);
        
        return true;
    }

    public bool HasLineOfSight(Unit attacker, Unit target)
    {
        if (!attacker.Position.HasValue || !target.Position.HasValue) return false;

        var start = attacker.Position.Value;
        var end = target.Position.Value;

        _map.HasLineOfSight(start,end);
        var lineOfSight = start.LineTo(end);

        foreach (var coordinates in lineOfSight)
        {
            if (coordinates == start || coordinates == end) continue;
            if (!IsHexFree(coordinates)) return false; //Should be more complicated than boolean as can also be partial
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

    public IEnumerable<Hex> GetHexes()
    {
        return _map.GetHexes();
    }

    private bool IsHexFree(HexCoordinates coordinates)
    {
        return _deployedUnits.Find(u => u.Position == coordinates)==null;
    }
}
