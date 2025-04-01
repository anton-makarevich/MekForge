using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Weapons;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Weapons;

public class AmmoTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Assert
        ammo.Name.ShouldBe("MachineGun Ammo");
        ammo.Type.ShouldBe(AmmoType.MachineGun);
        ammo.RemainingShots.ShouldBe(200);
        ammo.MountedAtSlots.ToList().Count.ShouldBe(0);
        ammo.Size.ShouldBe(1);
    }

    [Fact]
    public void UseShot_DecrementsRemainingShots()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Act
        ammo.UseShot();

        // Assert
        ammo.RemainingShots.ShouldBe(199);
    }

    [Fact]
    public void UseShot_WhenEmpty_DoesNotDecrementBelowZero()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 0);

        // Act
        ammo.UseShot();

        // Assert
        ammo.RemainingShots.ShouldBe(0);
    }

    [Fact]
    public void Hit_DestroysAmmo()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Act
        ammo.Hit();

        // Assert
        ammo.IsDestroyed.ShouldBeTrue();
    }
}
