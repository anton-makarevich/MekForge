using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal;

public class SensorsTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var sensors = new Sensors();

        // Assert
        sensors.Name.Should().Be("Sensors");
        sensors.MountedAtSlots.Should().HaveCount(2);
        sensors.MountedAtSlots.Should().ContainInOrder(1,4);
        sensors.IsDestroyed.Should().BeFalse();
    }
}