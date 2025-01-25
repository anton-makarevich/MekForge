using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class HeatSinkTests
{
    [Fact]
    public void Constructor_DefaultValues()
    {
        // Arrange & Act
        var heatSink = new HeatSink();

        // Assert
        heatSink.Name.ShouldBe("Heat Sink");
        heatSink.MountedAtSlots.ShouldBeEmpty();
        heatSink.HeatDissipation.ShouldBe(1);
        heatSink.IsDestroyed.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var heatSink = new HeatSink(dissipation: 2, name: "Double Heat Sink");

        // Assert
        heatSink.Name.ShouldBe("Double Heat Sink");
        heatSink.MountedAtSlots.ShouldBeEmpty();
        heatSink.HeatDissipation.ShouldBe(2);
        heatSink.IsDestroyed.ShouldBeFalse();
    }

    [Fact]
    public void Hit_DestroysHeatSink()
    {
        // Arrange
        var heatSink = new HeatSink();

        // Act
        heatSink.Hit();

        // Assert
        heatSink.IsDestroyed.ShouldBeTrue();
    }
}
