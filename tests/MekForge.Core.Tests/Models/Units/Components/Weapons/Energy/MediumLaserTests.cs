using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Energy;

public class MediumLaserTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var laser = new MediumLaser();

        // Assert
        laser.Name.Should().Be("Medium Laser");
        laser.RequiredSlots.Length.Should().Be(1);
        laser.Heat.Should().Be(3);
        laser.Damage.Should().Be(5);
        laser.BattleValue.Should().Be(46);
        laser.AmmoType.Should().Be(AmmoType.None);
    }

    [Fact]
    public void Hit_DestroysLaser()
    {
        // Arrange
        var laser = new MediumLaser();

        // Act
        laser.Hit();

        // Assert
        laser.IsDestroyed.Should().BeTrue();
    }
}
