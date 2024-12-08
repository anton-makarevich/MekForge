using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Missile;

public class LRM5Tests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var lrm5 = new LRM5();

        // Assert
        lrm5.Name.Should().Be("LRM-10");
        lrm5.Size.Should().Be(1);
        lrm5.Heat.Should().Be(2);
        lrm5.Damage.Should().Be(5); // Total damage for all missiles
        lrm5.BattleValue.Should().Be(45);
        lrm5.AmmoType.Should().Be(AmmoType.LRM5);
        lrm5.Clusters.Should().Be(1); 
        lrm5.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Hit_DestroysLRM5()
    {
        // Arrange
        var lrm5 = new LRM5();

        // Act
        lrm5.Hit();

        // Assert
        lrm5.IsDestroyed.Should().BeTrue();
    }
}
