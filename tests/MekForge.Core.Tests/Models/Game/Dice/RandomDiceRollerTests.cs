using FluentAssertions;
using Sanet.MekForge.Core.Models.Game.Dice;

namespace Sanet.MekForge.Core.Tests.Models.Game.Dice;

public class RandomDiceRollerTests
{
    [Fact]
    public void Roll_ShouldReturnDiceResult_WithValueInRange()
    {
        // Arrange
        var roller = new RandomDiceRoller();

        // Act
        var result = roller.Roll();

        // Assert
        result.Result.Should().BeGreaterThan(0).And.BeLessThan(7);
    }

    [Fact]
    public void Roll2D_ShouldReturnListOfTwoDiceResults()
    {
        // Arrange
        var roller = new RandomDiceRoller();

        // Act
        var results = roller.Roll2D();

        // Assert
        results.Should().HaveCount(2);
        results.All(r => r.Result is > 0 and < 7).Should().BeTrue();
    }
}