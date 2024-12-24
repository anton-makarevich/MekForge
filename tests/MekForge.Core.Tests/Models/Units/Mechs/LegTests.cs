using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class LegTests
{
    [Fact]
    public void Leg_ShouldBeInitializedCorrectly()
    {
        var leftLeg = new Leg(PartLocation.LeftLeg, 8, 4);
        var rightLeg = new Leg(PartLocation.RightLeg, 8, 4);

        leftLeg.Location.Should().Be(PartLocation.LeftLeg);
        leftLeg.MaxArmor.Should().Be(8);
        leftLeg.MaxStructure.Should().Be(4);

        rightLeg.Location.Should().Be(PartLocation.RightLeg);
        rightLeg.MaxArmor.Should().Be(8);
        rightLeg.MaxStructure.Should().Be(4);
    }
}
