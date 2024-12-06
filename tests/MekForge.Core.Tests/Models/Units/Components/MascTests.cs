using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class MascTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var masc = new Masc("MASC", 2);

        // Assert
        masc.Name.Should().Be("MASC");
        masc.RequiredSlots.Length.Should().Be(2);
        masc.IsDestroyed.Should().BeFalse();
        masc.IsActive.Should().BeFalse(); // MASC starts deactivated
    }

    [Fact]
    public void Hit_DestroysAndDeactivatesComponent()
    {
        // Arrange
        var masc = new Masc("MASC", 2);
        masc.Activate();

        // Act
        masc.Hit();

        // Assert
        masc.IsDestroyed.Should().BeTrue();
        masc.IsActive.Should().BeFalse();
    }
}
