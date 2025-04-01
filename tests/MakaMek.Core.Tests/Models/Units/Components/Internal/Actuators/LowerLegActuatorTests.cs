using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Internal.Actuators;

public class LowerLegActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new LowerLegActuator();

        // Assert
        actuator.Name.ShouldBe("Lower Leg");
        actuator.MountedAtSlots.ToList().Count.ShouldBe(1);
        actuator.MountedAtSlots.ShouldBe([2]);
        actuator.IsDestroyed.ShouldBeFalse();
    }
}