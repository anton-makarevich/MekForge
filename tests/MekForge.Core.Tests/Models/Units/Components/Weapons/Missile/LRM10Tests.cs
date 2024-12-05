using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Missile;

public class LRM10Tests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var lrm10 = new LRM10();

        // Assert
        lrm10.Name.Should().Be("LRM-10");
        lrm10.Slots.Should().Be(2);
        lrm10.Heat.Should().Be(4);
        lrm10.Damage.Should().Be(10); // Total damage for all missiles
        lrm10.BattleValue.Should().Be(90);
        lrm10.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void ApplyDamage_DestroysLRM10()
    {
        // Arrange
        var lrm10 = new LRM10();

        // Act
        lrm10.ApplyDamage();

        // Assert
        lrm10.IsDestroyed.Should().BeTrue();
    }
}
