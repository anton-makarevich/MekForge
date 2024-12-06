using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class FootActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new FootActuator();

        // Assert
        actuator.Name.Should().Be("Foot Actuator");
        actuator.MountedAtSlots.Should().HaveCount(1);
        actuator.MountedAtSlots.Should().ContainInOrder(3);
        actuator.IsDestroyed.Should().BeFalse();
    }
}