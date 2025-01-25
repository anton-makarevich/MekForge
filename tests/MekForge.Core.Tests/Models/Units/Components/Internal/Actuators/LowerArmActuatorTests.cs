using Shouldly;
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
        actuator.Name.ShouldBe("Lower Arm");
        actuator.MountedAtSlots.ToList().Count.ShouldBe(1);
        actuator.MountedAtSlots.ShouldBe([2]);
        actuator.IsDestroyed.ShouldBeFalse();
    }
}