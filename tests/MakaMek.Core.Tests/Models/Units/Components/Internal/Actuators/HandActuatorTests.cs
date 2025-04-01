using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Internal.Actuators;

public class HandActuatorTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var actuator = new HandActuator();

        // Assert
        actuator.Name.ShouldBe("Hand Actuator");
        actuator.MountedAtSlots.ToList().Count.ShouldBe(1);
        actuator.MountedAtSlots.ShouldBe([3]);
        actuator.IsDestroyed.ShouldBeFalse();
    }
}