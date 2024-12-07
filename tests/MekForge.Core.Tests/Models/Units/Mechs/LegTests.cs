using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class LegTests
{
    [Theory]
    [InlineData(PartLocation.LeftSide)]
    [InlineData(PartLocation.RightSide)]
    public void Constructor_InitializesCorrectly(PartLocation location)
    {
        // Arrange & Act
        var leg = new Leg(location, 10, 5);

        // Assert
        leg.Name.Should().Be($"{location} Leg");
        leg.Location.Should().Be(location);
        leg.MaxArmor.Should().Be(10);
        leg.CurrentArmor.Should().Be(10);
        leg.MaxStructure.Should().Be(5);
        leg.CurrentStructure.Should().Be(5);
        leg.TotalSlots.Should().Be(12);

        // Verify default components
        leg.GetComponent<Hip>().Should().NotBeNull();
        leg.GetComponent<UpperLegActuator>().Should().NotBeNull();
        leg.GetComponent<LowerLegActuator>().Should().NotBeNull();
        leg.GetComponent<FootActuator>().Should().NotBeNull();
    }
}
