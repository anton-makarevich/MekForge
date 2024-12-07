using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class ArmTests
{
    [Theory]
    [InlineData(PartLocation.LeftSide)]
    [InlineData(PartLocation.RightSide)]
    public void Constructor_InitializesCorrectly(PartLocation location)
    {
        // Arrange & Act
        var arm = new Arm(location, 10, 5);

        // Assert
        arm.Name.Should().Be($"{location} Arm");
        arm.Location.Should().Be(location);
        arm.MaxArmor.Should().Be(10);
        arm.CurrentArmor.Should().Be(10);
        arm.MaxStructure.Should().Be(5);
        arm.CurrentStructure.Should().Be(5);
        arm.TotalSlots.Should().Be(12);

        // Verify default components
        arm.GetComponent<Shoulder>().Should().NotBeNull();
    }
}