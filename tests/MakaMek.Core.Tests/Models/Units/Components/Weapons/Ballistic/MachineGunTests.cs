using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Weapons;
using Sanet.MakaMek.Core.Models.Units.Components.Weapons.Ballistic;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Weapons.Ballistic;

public class MachineGunTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var machineGun = new MachineGun();

        // Assert
        machineGun.Name.ShouldBe("Machine Gun");
        machineGun.Size.ShouldBe(1);
        machineGun.Damage.ShouldBe(2);
        machineGun.Heat.ShouldBe(0);
        machineGun.MinimumRange.ShouldBe(0);
        machineGun.ShortRange.ShouldBe(1);
        machineGun.MediumRange.ShouldBe(2);
        machineGun.LongRange.ShouldBe(3);
        machineGun.Type.ShouldBe(WeaponType.Ballistic);
        machineGun.BattleValue.ShouldBe(5);
        machineGun.AmmoType.ShouldBe(AmmoType.MachineGun);
        machineGun.IsDestroyed.ShouldBeFalse();
        machineGun.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var machineGun = new MachineGun();

        // Act
        machineGun.Hit();

        // Assert
        machineGun.IsDestroyed.ShouldBeTrue();
    }

    [Fact]
    public void Activate_Deactivate_TogglesIsActive()
    {
        // Arrange
        var machineGun = new MachineGun();

        // Act & Assert
        machineGun.IsActive.ShouldBeTrue(); // Default state

        machineGun.Deactivate();
        machineGun.IsActive.ShouldBeFalse();

        machineGun.Activate();
        machineGun.IsActive.ShouldBeTrue();
    }
}
