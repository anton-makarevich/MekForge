using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal.Actuators;

public class UpperLegActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new UpperLegActuator();

        // Assert
        actuator.Name.ShouldBe("Upper Leg");
        actuator.MountedAtSlots.ToList().Count.ShouldBe(1);
        actuator.MountedAtSlots.ShouldBe([1]);
        actuator.IsDestroyed.ShouldBeFalse();
    }
}