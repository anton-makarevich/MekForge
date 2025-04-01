using Shouldly;
using Sanet.MakaMek.Core.Models.Game.Dice;

namespace Sanet.MakaMek.Core.Tests.Models.Game.Dice;

public class RandomDiceRollerTests
{
    [Fact]
    public void Roll_ShouldReturnDiceResult_WithValueInRange()
    {
        // Arrange
        var roller = new RandomDiceRoller();

        // Act
        var result = roller.RollD6();

        // Assert
        result.Result.ShouldBeGreaterThan(0);
        result.Result.ShouldBeLessThan(7);
    }

    [Fact]
    public void Roll2D_ShouldReturnListOfTwoDiceResults()
    {
        // Arrange
        var roller = new RandomDiceRoller();

        // Act
        var results = roller.Roll2D6();

        // Assert
        results.Count.ShouldBe(2);
        results.All(r => r.Result is > 0 and < 7).ShouldBeTrue();
    }
}