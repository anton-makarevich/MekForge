using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class LowerArmActuatorTests
{

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new LowerArmActuator();

        // Assert
        actuator.Name.Should().Be("Lower Arm");
        actuator.MountedAtSlots.Should().HaveCount(1);
        actuator.MountedAtSlots.Should().ContainInOrder(2);
        actuator.IsDestroyed.Should().BeFalse();
    }
}