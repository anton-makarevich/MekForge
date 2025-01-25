using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Energy;

public class MediumLaserTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var laser = new MediumLaser();

        // Assert
        laser.Name.ShouldBe("Medium Laser");
        laser.Size.ShouldBe(1);
        laser.Heat.ShouldBe(3);
        laser.Damage.ShouldBe(5);
        laser.BattleValue.ShouldBe(46);
        laser.AmmoType.ShouldBe(AmmoType.None);
    }

    [Fact]
    public void Hit_DestroysLaser()
    {
        // Arrange
        var laser = new MediumLaser();

        // Act
        laser.Hit();

        // Assert
        laser.IsDestroyed.ShouldBeTrue();
    }
}
