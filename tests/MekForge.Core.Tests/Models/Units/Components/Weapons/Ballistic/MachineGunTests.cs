using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Ballistic;

public class MachineGunTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var machineGun = new MachineGun();

        // Assert
        machineGun.Name.Should().Be("Machine Gun");
        machineGun.Size.Should().Be(1);
        machineGun.Damage.Should().Be(2);
        machineGun.Heat.Should().Be(0);
        machineGun.MinimumRange.Should().Be(0);
        machineGun.ShortRange.Should().Be(1);
        machineGun.MediumRange.Should().Be(2);
        machineGun.LongRange.Should().Be(3);
        machineGun.Type.Should().Be(WeaponType.Ballistic);
        machineGun.BattleValue.Should().Be(5);
        machineGun.AmmoType.Should().Be(AmmoType.MachineGun);
        machineGun.IsDestroyed.Should().BeFalse();
        machineGun.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var machineGun = new MachineGun();

        // Act
        machineGun.Hit();

        // Assert
        machineGun.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Activate_Deactivate_TogglesIsActive()
    {
        // Arrange
        var machineGun = new MachineGun();

        // Act & Assert
        machineGun.IsActive.Should().BeTrue(); // Default state

        machineGun.Deactivate();
        machineGun.IsActive.Should().BeFalse();

        machineGun.Activate();
        machineGun.IsActive.Should().BeTrue();
    }
}
