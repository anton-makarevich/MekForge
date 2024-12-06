using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class LowerLegActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new LowerLegActuator();

        // Assert
        actuator.Name.Should().Be("Lower Leg");
        actuator.MountedAtSlots.Should().HaveCount(1);
        actuator.MountedAtSlots.Should().ContainInOrder(2);
        actuator.IsDestroyed.Should().BeFalse();
    }
}