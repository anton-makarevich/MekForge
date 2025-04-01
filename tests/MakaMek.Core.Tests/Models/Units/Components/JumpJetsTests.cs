using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components;

public class JumpJetsFacts
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var jumpJets = new JumpJets();

        // Assert
        jumpJets.Name.ShouldBe("Jump Jets");
        jumpJets.Size.ShouldBe(1);
        jumpJets.JumpMp.ShouldBe(1);
        jumpJets.IsDestroyed.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithCustomJumpMp_SetsCorrectValues()
    {
        // Arrange & Act
        var jumpJets = new JumpJets(2);

        // Assert
        jumpJets.Name.ShouldBe("Jump Jets");
        jumpJets.Size.ShouldBe(1);
        jumpJets.JumpMp.ShouldBe(2);
        jumpJets.IsDestroyed.ShouldBeFalse();
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var jumpJets = new JumpJets();

        // Act
        jumpJets.Hit();

        // Assert
        jumpJets.IsDestroyed.ShouldBeTrue();
    }
}
