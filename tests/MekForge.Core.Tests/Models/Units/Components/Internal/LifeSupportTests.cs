using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal;

public class LifeSupportTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var lifeSupport = new LifeSupport();

        // Assert
        lifeSupport.Name.Should().Be("Life Support");
        lifeSupport.MountedAtSlots.Should().HaveCount(2);
        lifeSupport.MountedAtSlots.Should().ContainInOrder(0, 5);
        lifeSupport.IsDestroyed.Should().BeFalse();
    }
}
