using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Internal;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Internal;

public class LifeSupportTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var lifeSupport = new LifeSupport();

        // Assert
        lifeSupport.Name.ShouldBe("Life Support");
        lifeSupport.MountedAtSlots.ToList().Count.ShouldBe(2);
        lifeSupport.MountedAtSlots.ShouldBe([0, 5]);
        lifeSupport.IsDestroyed.ShouldBeFalse();
    }
}
