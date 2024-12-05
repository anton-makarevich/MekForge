namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;

public class AC5 : Weapon
{
    private static readonly int[] AC5Slots = { 0, 1, 2, 3 };

    public AC5() : base(
        name: "AC/5",
        slots: AC5Slots,
        damage: 5,
        heat: 1,
        minimumRange: 3,
        shortRange: 6,
        mediumRange: 12,
        longRange: 18,
        type: WeaponType.Ballistic,
        battleValue: 123,
        ammoType: AmmoType.AC5)
    {
    }
}
