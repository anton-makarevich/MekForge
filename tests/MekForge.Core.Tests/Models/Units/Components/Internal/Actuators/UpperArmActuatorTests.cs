using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class UpperArmActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new UpperArmActuator();

        // Assert
        actuator.Name.Should().Be("Upper Arm Actuator");
        actuator.MountedAtSlots.Should().HaveCount(1);
        actuator.MountedAtSlots.Should().ContainInOrder(1);
        actuator.IsDestroyed.Should().BeFalse();
    }
}