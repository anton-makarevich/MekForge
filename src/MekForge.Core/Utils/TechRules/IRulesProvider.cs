using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Utils.TechRules;

public interface IRulesProvider
{
    Dictionary<PartLocation, int> GetStructureValues(int tonnage);
    int GetAmmoRounds(AmmoType ammoType);
}