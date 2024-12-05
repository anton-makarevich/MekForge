namespace Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;

public class MachineGun : Weapon
{
    private static readonly int[] MachineGunSlots = { 0 };

    public MachineGun() : base(
        name: "Machine Gun",
        slots: MachineGunSlots,
        damage: 2,
        heat: 0,
        minimumRange: 0,
        shortRange: 1,
        mediumRange: 2,
        longRange: 3,
        type: WeaponType.Ballistic,
        battleValue: 5,
        ammoType: AmmoType.MachineGun)
    {
    }
}
