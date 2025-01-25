using Shouldly;
using Sanet.MekForge.Core.Models.Game.Dice;

namespace Sanet.MekForge.Core.Tests.Models.Game.Dice;

public class DiceResultTests
{
    [Fact]
    public void SettingResult_ShouldThrowException_WhenValueIsOutOfRange()
    {
        // Arrange
        var diceResult = new DiceResult();

        // Act & Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => diceResult.Result = 7);
        ex.ParamName.ShouldBe("value");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 6)]
    public void SettingResult_ShouldSetValue_WhenValueIsInRange(int input, int expected)
    {
        // Arrange
        var diceResult = new DiceResult();

        // Act
        diceResult.Result = input;

        // Assert
        diceResult.Result.ShouldBe(expected);
    }
}