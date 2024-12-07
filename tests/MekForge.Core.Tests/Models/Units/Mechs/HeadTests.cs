using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class HeadTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var head = new Head(PartLocation.Center, 10, 5);

        // Assert
        head.Name.Should().Be("Head");
        head.Location.Should().Be(PartLocation.Center);
        head.MaxArmor.Should().Be(10);
        head.CurrentArmor.Should().Be(10);
        head.MaxStructure.Should().Be(5);
        head.CurrentStructure.Should().Be(5);
        head.TotalSlots.Should().Be(12);

        // Verify default components
        head.GetComponent<LifeSupport>().Should().NotBeNull();
        head.GetComponent<Sensors>().Should().NotBeNull();
        head.GetComponent<Cockpit>().Should().NotBeNull();
    }
}
