using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class HeatSinkTests
{
    [Fact]
    public void Constructor_DefaultValues()
    {
        // Arrange & Act
        var heatSink = new HeatSink();

        // Assert
        heatSink.Name.Should().Be("Heat Sink");
        heatSink.MountedAtSlots.Should().BeEmpty();
        heatSink.HeatDissipation.Should().Be(1);
        heatSink.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var heatSink = new HeatSink(dissipation: 2, name: "Double Heat Sink");

        // Assert
        heatSink.Name.Should().Be("Double Heat Sink");
        heatSink.MountedAtSlots.Should().BeEmpty();
        heatSink.HeatDissipation.Should().Be(2);
        heatSink.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Hit_DestroysHeatSink()
    {
        // Arrange
        var heatSink = new HeatSink();

        // Act
        heatSink.Hit();

        // Assert
        heatSink.IsDestroyed.Should().BeTrue();
    }
}
