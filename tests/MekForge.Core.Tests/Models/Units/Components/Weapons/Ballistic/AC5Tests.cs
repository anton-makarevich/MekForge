using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Ballistic;

public class AC5Tests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var ac5 = new AC5();

        // Assert
        ac5.Name.Should().Be("AC/5");
        ac5.Slots.Should().Be(4);
        ac5.Heat.Should().Be(1);
        ac5.Damage.Should().Be(5);
        ac5.BattleValue.Should().Be(75);
        ac5.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void ApplyDamage_DestroysAC5()
    {
        // Arrange
        var ac5 = new AC5();

        // Act
        ac5.ApplyDamage();

        // Assert
        ac5.IsDestroyed.Should().BeTrue();
    }
}
