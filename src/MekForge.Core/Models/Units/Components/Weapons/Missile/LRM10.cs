namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;

public class LRM10 : Weapon
{
    public LRM10() : base(
        name: "LRM-10",
        damage: 10, // 1 damage per missile, 10 missiles
        heat: 4,
        minimumRange: 6,
        shortRange: 7,
        mediumRange: 14,
        longRange: 21,
        type: WeaponType.Missile,
        battleValue: 90,
        ammoType: AmmoType.LRM10)
    {
    }
}