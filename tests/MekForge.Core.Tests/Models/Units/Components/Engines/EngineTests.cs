using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components.Engines;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Engines;

public class EngineTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var engine = new Engine(100);

        // Assert
        engine.Name.ShouldBe("Fusion Engine 100");
        engine.Rating.ShouldBe(100);
        engine.MountedAtSlots.ToList().Count.ShouldBe(6);
        engine.MountedAtSlots.ShouldBe([0,1,2,7,8,9]);
        engine.IsDestroyed.ShouldBeFalse();
    }
}
