using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Internal;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Internal;

public class SensorsTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var sensors = new Sensors();

        // Assert
        sensors.Name.ShouldBe("Sensors");
        sensors.MountedAtSlots.ToList().Count.ShouldBe(2);
        sensors.MountedAtSlots.ShouldBe([1,4]);
        sensors.IsDestroyed.ShouldBeFalse();
    }
}