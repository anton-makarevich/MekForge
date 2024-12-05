namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;

public class AC5 : Weapon
{
    public AC5() : base(
        name: "AC/5",
        slots: 4,
        damage: 5,
        heat: 1,
        minimumRange: 0,
        shortRange: 6,
        mediumRange: 12,
        longRange: 18,
        type: WeaponType.Ballistic,
        battleValue: 75)
    {
    }
}
