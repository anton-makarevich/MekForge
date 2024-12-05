namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;

public class MediumLaser : Weapon
{
    public MediumLaser() : base(
        name: "Medium Laser",
        slots: 1,
        damage: 5,
        heat: 3,
        minimumRange: 0,
        shortRange: 3,
        mediumRange: 6,
        longRange: 9,
        type: WeaponType.Energy,
        battleValue: 46)
    {
    }
}
