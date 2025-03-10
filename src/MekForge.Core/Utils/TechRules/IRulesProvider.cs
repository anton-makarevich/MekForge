using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Utils.TechRules;

public interface IRulesProvider
{
    Dictionary<PartLocation, int> GetStructureValues(int tonnage);
    int GetAmmoRounds(AmmoType ammoType);

    /// <summary>
    /// Gets the modifier for attacker's movement type
    /// </summary>
    int GetAttackerMovementModifier(MovementType movementType);

    /// <summary>
    /// Gets the modifier based on how many hexes the target has moved
    /// </summary>
    int GetTargetMovementModifier(int hexesMoved);

    /// <summary>
    /// Gets the modifier for firing at a specific range bracket
    /// </summary>
    int GetRangeModifier(WeaponRange  rangeType, int rangeValue, int distance);

    /// <summary>
    /// Gets the modifier based on attacker's current heat level
    /// </summary>
    int GetHeatModifier(int currentHeat);

    /// <summary>
    /// Gets the to-hit modifier for a specific terrain type
    /// </summary>
    int GetTerrainToHitModifier(string terrainType);

    /// <summary>
    /// Gets the modifier for firing at a secondary target
    /// </summary>
    /// <param name="isFrontArc">Whether the target is in the front arc</param>
    /// <returns>The modifier value to apply</returns>
    int GetSecondaryTargetModifier(bool isFrontArc);
}