using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal;

public class CockpitTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var cockpit = new Cockpit();

        // Assert
        cockpit.Name.ShouldBe("Cockpit");
        cockpit.MountedAtSlots.ToList().Count.ShouldBe(1);
        cockpit.MountedAtSlots.ShouldBe([2]);
        cockpit.IsDestroyed.ShouldBeFalse();
    }
}