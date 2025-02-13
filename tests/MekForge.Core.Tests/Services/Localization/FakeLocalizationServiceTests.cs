using Shouldly;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Services.Localization;

public class FakeLocalizationServiceTests
{
    [Theory]
    [InlineData("Command_JoinGame", "{0} has joined game with {1} units.")]
    [InlineData("Command_MoveUnit", "{0} moved {1} to {2} facing {3} using {4}.")]
    [InlineData("Command_DeployUnit", "{0} deployed {1} to {2} facing {3}.")]
    [InlineData("Command_RollDice", "{0} rolls")]
    [InlineData("Command_DiceRolled", "{0} rolled {1}.")]
    [InlineData("Command_UpdatePlayerStatus", "{0}'s status is {1}.")]
    [InlineData("Command_ChangePhase", "Game changed phase to {0}.")]
    [InlineData("Command_ChangeActivePlayer", "{0}'s turn.")]
    [InlineData("Command_ChangeActivePlayerUnits", "{0}'s turn to play {1} units.")]
    [InlineData("Command_WeaponConfiguration_TorsoRotation", "{0}'s {1} rotates torso to {2}")]
    [InlineData("Command_WeaponConfiguration_ArmsFlip", "{0}'s {1} flips arms {2}")]
    [InlineData("Direction_Forward", "forward")]
    [InlineData("Direction_Backward", "backward")]
    public void GetString_ShouldReturnCorrectString(string key, string expectedValue)
    {
        // Arrange
        var localizationService = new FakeLocalizationService();

        // Act
        var result = localizationService.GetString(key);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void GetString_ShouldThrowArgumentOutOfRangeException_WhenKeyNotFound()
    {
        // Arrange
        var localizationService = new FakeLocalizationService();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => localizationService.GetString("NonExistentKey"));
    }
}