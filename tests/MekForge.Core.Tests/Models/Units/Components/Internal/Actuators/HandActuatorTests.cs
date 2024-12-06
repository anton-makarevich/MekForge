using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class HandActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new HandActuator();

        // Assert
        actuator.Name.Should().Be("Hand Actuator");
        actuator.MountedAtSlots.Should().HaveCount(1);
        actuator.MountedAtSlots.Should().ContainInOrder(3);
        actuator.IsDestroyed.Should().BeFalse();
    }
}