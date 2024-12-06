using FluentAssertions;
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
        cockpit.Name.Should().Be("Cockpit");
        cockpit.MountedAtSlots.Should().HaveCount(1);
        cockpit.MountedAtSlots.Should().ContainInOrder(2);
        cockpit.IsDestroyed.Should().BeFalse();
    }
}