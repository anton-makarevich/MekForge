using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game.Combat;

/// <summary>
/// Classic BattleTech implementation of to-hit calculator using GATOR system
/// </summary>
public class ClassicToHitCalculator : IToHitCalculator
{
    private readonly IRulesProvider _rules;

    public ClassicToHitCalculator(IRulesProvider rules)
    {
        _rules = rules;
    }

    public int GetToHitNumber(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        var breakdown = GetModifierBreakdown(attacker, target, weapon, map);
        return breakdown.Total;
    }

    public ToHitBreakdown GetModifierBreakdown(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        var hasLos = map.HasLineOfSight(attacker.Position!.Value.Coordinates, target.Position!.Value.Coordinates);
        var distance = attacker.Position!.Value.Coordinates.DistanceTo(target.Position!.Value.Coordinates);
        var range = weapon.GetRangeBracket(distance);
        var rangeValue = range switch
        {
            WeaponRange.Minimum => weapon.MinimumRange,
            WeaponRange.Short => weapon.ShortRange,
            WeaponRange.Medium => weapon.MediumRange,
            WeaponRange.Long => weapon.LongRange,
            WeaponRange.OutOfRange => weapon.LongRange+1,
            _ => throw new ArgumentException($"Unknown weapon range: {range}")
        };
        var otherModifiers = GetDetailedOtherModifiers(attacker, target, weapon, map);
        var terrainModifiers = GetTerrainModifiers(attacker, target, map);

        return new ToHitBreakdown
        {
            GunneryBase = new GunneryAttackModifier
            {
                Value = attacker.Crew!.Gunnery
            },
            AttackerMovement = new AttackerMovementModifier
            {
                Value = _rules.GetAttackerMovementModifier(attacker.MovementTypeUsed ?? 
                    throw new Exception("Attacker's Movement Type is undefined")),
                MovementType = attacker.MovementTypeUsed.Value
            },
            TargetMovement = new TargetMovementModifier
            {
                Value = _rules.GetTargetMovementModifier(target.DistanceCovered),
                HexesMoved = target.DistanceCovered
            },
            OtherModifiers = otherModifiers,
            RangeModifier = new RangeAttackModifier
            {
                Value = _rules.GetRangeModifier(range, rangeValue, distance),
                Range = range,
                Distance = distance,
                WeaponName = weapon.Name
            },
            TerrainModifiers = terrainModifiers,
            HasLineOfSight = hasLos
        };
    }

    private IReadOnlyList<AttackModifier> GetDetailedOtherModifiers(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        var modifiers = new List<AttackModifier> {
            new HeatAttackModifier
            {
                Value = _rules.GetHeatModifier(attacker.CurrentHeat),
                HeatLevel = attacker.CurrentHeat
            }
        };

        // TODO: Add other modifiers like:
        // - Attacker damage (actuators)
        // - Multiple targets
        // - Special terrain effects

        return modifiers;
    }

    private IReadOnlyList<TerrainAttackModifier> GetTerrainModifiers(Unit attacker, Unit target, BattleMap map)
    {
        var hexes = map.GetHexesAlongLineOfSight(
            attacker.Position!.Value.Coordinates,
            target.Position!.Value.Coordinates);

        return hexes
            .Skip(1) // Skip attacker's hex
            .SelectMany(hex => hex.GetTerrains()
                .Select(terrain => new TerrainAttackModifier
                {
                    Value = _rules.GetTerrainToHitModifier(terrain.Id),
                    Location = hex.Coordinates,
                    TerrainId = terrain.Id
                }))
            .Where(modifier => modifier.Value != 0)
            .ToList();
    }
}
