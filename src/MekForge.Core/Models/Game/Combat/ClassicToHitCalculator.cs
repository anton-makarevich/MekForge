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

    public int GetToHitModifier(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        if (!map.HasLineOfSight(attacker.Position!.Value.Coordinates, target.Position!.Value.Coordinates))
            return int.MaxValue; // Cannot hit without line of sight

        var distance = attacker.Position!.Value.Coordinates.DistanceTo(target.Position!.Value.Coordinates);
        var range = weapon.GetRangeBracket(distance);

        if (range == WeaponRange.OutOfRange)
            return int.MaxValue; // Cannot hit if target is out of range

        return attacker.Crew!.Gunnery + // Base gunnery skill (G)
               _rules.GetAttackerMovementModifier(attacker.MovementTypeUsed?? throw new Exception("Attacker's Movement Type is undefined") ) + // Attacker movement (A)
               _rules.GetTargetMovementModifier(target.DistanceCovered) + // Target movement (T)
               GetOtherModifiers(attacker, target, weapon, map) + // Other modifiers (O)
               _rules.GetRangeModifier(range); // Range modifier (R)
    }

    public ToHitBreakdown GetModifierBreakdown(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        var hasLos = map.HasLineOfSight(attacker.Position!.Value.Coordinates, target.Position!.Value.Coordinates);
        var distance = attacker.Position!.Value.Coordinates.DistanceTo(target.Position!.Value.Coordinates);
        var range = weapon.GetRangeBracket(distance);
        var otherModifiers = GetDetailedOtherModifiers(attacker, target, weapon, map);
        var terrainModifiers = GetTerrainModifiers(attacker, target, map);

        return new ToHitBreakdown
        {
            GunneryBase = new AttackModifier(AttackModifierType.Gunnery, attacker.Crew!.Gunnery),
            AttackerMovement = new AttackModifier(
                AttackModifierType.AttackerMovement, 
                _rules.GetAttackerMovementModifier(attacker.MovementTypeUsed ?? throw new Exception("Attacker's Movement Type is undefined"))),
            TargetMovement = new AttackModifier(
                AttackModifierType.TargetMovement,
                _rules.GetTargetMovementModifier(target.DistanceCovered)),
            OtherModifiers = otherModifiers,
            RangeModifier = new AttackModifier(
                AttackModifierType.Range,
                _rules.GetRangeModifier(range)),
            TerrainModifiers = terrainModifiers,
            HasLineOfSight = hasLos
        };
    }

    private int GetOtherModifiers(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        return GetDetailedOtherModifiers(attacker, target, weapon, map).Sum(m => m.Value) +
               GetTerrainModifiers(attacker, target, map).Sum(t => t.Value);
    }

    private IReadOnlyList<AttackModifier> GetDetailedOtherModifiers(Unit attacker, Unit target, Weapon weapon, BattleMap map)
    {
        var modifiers = new List<AttackModifier> {
            new(AttackModifierType.Heat, _rules.GetHeatModifier(attacker.CurrentHeat))
        };

        // TODO: Add other modifiers like:
        // - Attacker damage (actuators)
        // - Multiple targets
        // - Special terrain effects

        return modifiers;
    }

    private IReadOnlyList<AttackModifier> GetTerrainModifiers(Unit attacker, Unit target, BattleMap map)
    {
        var hexes = map.GetHexesAlongLineOfSight(
            attacker.Position!.Value.Coordinates,
            target.Position!.Value.Coordinates);

        return hexes
            .Skip(1) // Skip attacker's hex
            .SelectMany(hex => hex.GetTerrains()
                .Select(terrain => new AttackModifier(
                    AttackModifierType.TerrainEffect,
                    _rules.GetTerrainToHitModifier(terrain.Id))))
            .Where(modifier => modifier.Value != 0)
            .ToList();
    }
}
