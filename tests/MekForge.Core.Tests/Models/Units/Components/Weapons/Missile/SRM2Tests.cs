using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Missile;

public class SRM2Tests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var srm2 = new SRM2();

        // Assert
        srm2.Name.Should().Be("SRM-2");
        srm2.Size.Should().Be(1);
        srm2.Heat.Should().Be(1);
        srm2.Damage.Should().Be(4); // Total damage for all missiles
        srm2.BattleValue.Should().Be(25);
        srm2.AmmoType.Should().Be(AmmoType.SRM2);
        srm2.MinimumRange.Should().Be(0);
        srm2.ShortRange.Should().Be(3);
        srm2.MediumRange.Should().Be(6);
        srm2.LongRange.Should().Be(9);
        srm2.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Hit_DestroysSRM2()
    {
        // Arrange
        var srm2 = new SRM2();

        // Act
        srm2.Hit();

        // Assert
        srm2.IsDestroyed.Should().BeTrue();
    }
}
