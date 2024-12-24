using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class HeadTests
{
    [Fact]
    public void Head_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var head = new Head( 8, 3);

        // Assert
        head.Name.Should().Be("Head");
        head.Location.Should().Be(PartLocation.Head);
        head.MaxArmor.Should().Be(8);
        head.CurrentArmor.Should().Be(8);
        head.MaxStructure.Should().Be(3);
        head.CurrentStructure.Should().Be(3);
        head.TotalSlots.Should().Be(12);

        // Verify default components
        head.GetComponent<LifeSupport>().Should().NotBeNull();
        head.GetComponent<Sensors>().Should().NotBeNull();
        head.GetComponent<Cockpit>().Should().NotBeNull();
    }
}
