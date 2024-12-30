using FluentAssertions;
using Sanet.MekForge.Core.Models.Game.Dice;

namespace Sanet.MekForge.Core.Tests.Models.Game.Dice;

public class DiceResultTests
{
    [Fact]
    public void SettingResult_ShouldThrowException_WhenValueIsOutOfRange()
    {
        // Arrange
        var diceResult = new DiceResult();

        // Act
        Action act = () => diceResult.Result = 7;

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void SettingResult_ShouldSetValue_WhenValueIsInRange()
    {
        // Arrange
        var diceResult = new DiceResult
        {
            // Act
            Result = 5
        };

        // Assert
        diceResult.Result.Should().Be(5);
    }
}