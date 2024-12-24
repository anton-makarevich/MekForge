using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace MekForge.Core.Tests.Models.Units.Mechs;

public class CenterTorsoTests
{
    [Fact]
    public void CenterTorso_ShouldBeInitializedCorrectly()
    {
        var centerTorso = new CenterTorso(10, 2, 6);

        Assert.Equal(PartLocation.CenterTorso, centerTorso.Location);
        Assert.Equal(10, centerTorso.MaxArmor);
        Assert.Equal(2, centerTorso.MaxRearArmor);
        Assert.Equal(6, centerTorso.MaxStructure);
    }
}
