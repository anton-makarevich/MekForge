using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile
{
    public class SRM2 : Weapon
    {
        public SRM2() : base(
            name: "SRM-2",
            damage: 4, // 2 damage per missile, 2 missiles
            heat: 1,
            minimumRange: 0,
            shortRange: 3,
            mediumRange: 6,
            longRange: 9,
            type: WeaponType.Missile,
            battleValue: 25,
            ammoType: AmmoType.SRM2)
        {
        }
    }
}