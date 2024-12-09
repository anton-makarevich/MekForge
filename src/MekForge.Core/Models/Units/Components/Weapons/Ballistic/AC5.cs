namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;

public class AC5 : Weapon
{
    public AC5() : base(
        name: "AC/5",
        damage: 5,
        heat: 1,
        minimumRange: 3,
        shortRange: 6,
        mediumRange: 12,
        longRange: 18,
        type: WeaponType.Ballistic,
        battleValue: 123,
        size: 4,
        ammoType: AmmoType.AC5)
    {
    }
}
