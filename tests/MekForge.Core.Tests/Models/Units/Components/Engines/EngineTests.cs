using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Engines;

public class EngineTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var engine = new Engine("Test Engine", 100);

        // Assert
        engine.Name.Should().Be("Test Engine");
        engine.Rating.Should().Be(100);
        engine.MountedAtSlots.Should().HaveCount(6);
        engine.MountedAtSlots.Should().ContainInOrder(0,1,2,7,8,9);
        engine.IsDestroyed.Should().BeFalse();
    }
}
