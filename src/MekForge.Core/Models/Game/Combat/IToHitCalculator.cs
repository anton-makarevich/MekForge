using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Game.Combat;

/// <summary>
/// Interface for calculating to-hit modifiers using GATOR system
/// </summary>
public interface IToHitCalculator
{
    /// <summary>
    /// Gets the total to-hit modifier for a weapon attack
    /// </summary>
    int GetToHitModifier(Unit attacker, Unit target, Weapon weapon, BattleMap map);
    
    /// <summary>
    /// Gets detailed breakdown of all modifiers affecting the attack
    /// </summary>
    ToHitBreakdown GetModifierBreakdown(Unit attacker, Unit target, Weapon weapon, BattleMap map);
}
