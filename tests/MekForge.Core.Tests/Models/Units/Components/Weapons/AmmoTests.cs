using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons;

public class AmmoTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Assert
        ammo.Name.Should().Be("MachineGun Ammo");
        ammo.Type.Should().Be(AmmoType.MachineGun);
        ammo.RemainingShots.Should().Be(200);
        ammo.MountedAtSlots.Should().HaveCount(0);
        ammo.Size.Should().Be(1);
    }

    [Fact]
    public void UseShot_DecrementsRemainingShots()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Act
        ammo.UseShot();

        // Assert
        ammo.RemainingShots.Should().Be(199);
    }

    [Fact]
    public void UseShot_WhenEmpty_DoesNotDecrementBelowZero()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 0);

        // Act
        ammo.UseShot();

        // Assert
        ammo.RemainingShots.Should().Be(0);
    }

    [Fact]
    public void Hit_DestroysAmmo()
    {
        // Arrange
        var ammo = new Ammo(AmmoType.MachineGun, 200);

        // Act
        ammo.Hit();

        // Assert
        ammo.IsDestroyed.Should().BeTrue();
    }
}
