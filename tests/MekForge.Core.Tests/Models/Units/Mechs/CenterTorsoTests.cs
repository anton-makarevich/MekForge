using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class CenterTorsoTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var torso = new CenterTorso(10, 3, 5);

        // Assert
        torso.Name.Should().Be("Center Torso");
        torso.Location.Should().Be(PartLocation.Center);
        torso.MaxArmor.Should().Be(10);
        torso.CurrentArmor.Should().Be(10);
        torso.MaxRearArmor.Should().Be(3);
        torso.CurrentRearArmor.Should().Be(3);
        torso.MaxStructure.Should().Be(5);
        torso.CurrentStructure.Should().Be(5);
        torso.TotalSlots.Should().Be(12);

        // Verify default components
        torso.GetComponent<Gyro>().Should().NotBeNull();
    }
}
