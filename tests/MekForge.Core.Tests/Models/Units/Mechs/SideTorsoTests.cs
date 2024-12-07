using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class SideTorsoTests
{
    [Theory]
    [InlineData(PartLocation.LeftSide)]
    [InlineData(PartLocation.RightSide)]
    public void Constructor_InitializesCorrectly(PartLocation location)
    {
        // Arrange & Act
        var torso = new SideTorso(location, 10, 3, 5);

        // Assert
        torso.Name.Should().Be($"{location} Torso");
        torso.Location.Should().Be(location);
        torso.MaxArmor.Should().Be(10);
        torso.CurrentArmor.Should().Be(10);
        torso.MaxRearArmor.Should().Be(3);
        torso.CurrentRearArmor.Should().Be(3);
        torso.MaxStructure.Should().Be(5);
        torso.CurrentStructure.Should().Be(5);
        torso.TotalSlots.Should().Be(12);
    }
}