using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class JumpJetsFacts
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var jumpJets = new JumpJets();

        // Assert
        jumpJets.Name.Should().Be("Jump Jets");
        jumpJets.SlotsCount.Should().Be(0);
        jumpJets.JumpMp.Should().Be(1);
        jumpJets.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithCustomJumpMp_SetsCorrectValues()
    {
        // Arrange & Act
        var jumpJets = new JumpJets(2);

        // Assert
        jumpJets.Name.Should().Be("Jump Jets");
        jumpJets.SlotsCount.Should().Be(0);
        jumpJets.JumpMp.Should().Be(2);
        jumpJets.IsDestroyed.Should().BeFalse();
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var jumpJets = new JumpJets();

        // Act
        jumpJets.Hit();

        // Assert
        jumpJets.IsDestroyed.Should().BeTrue();
    }
}
