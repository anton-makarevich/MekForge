namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;

public class LRM5 : Weapon
{
    public LRM5() : base(
        name: "LRM-10",
        damage: 5, // 1 damage per missile, 5 missiles
        heat: 2,
        minimumRange: 6,
        shortRange: 7,
        mediumRange: 14,
        longRange: 21,
        type: WeaponType.Missile,
        battleValue: 45,
        clusters: 1,
        clusterSize: 5,
        ammoType: AmmoType.LRM5)
    {
    }
}
