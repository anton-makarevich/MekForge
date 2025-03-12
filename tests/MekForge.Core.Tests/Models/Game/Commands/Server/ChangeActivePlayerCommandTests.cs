using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class ChangeActivePlayerCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public ChangeActivePlayerCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    private ChangeActivePlayerCommand CreateCommand()
    {
        return new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitsToPlay = 1
        };
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = CreateCommand();

        _localizationService.GetString("Command_ChangeActivePlayerUnits").Returns("formatted active player command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBe("formatted active player command");
        _localizationService.Received(1).GetString("Command_ChangeActivePlayerUnits");
    }
    
    [Fact]
    public void Format_ShouldFormatCorrectly_WhenNoUnits()
    {
        // Arrange
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            UnitsToPlay = 0
        };

        _localizationService.GetString("Command_ChangeActivePlayer").Returns("formatted active player command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.ShouldBe("formatted active player command");
        _localizationService.Received(1).GetString("Command_ChangeActivePlayer");
    }
    
    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = _gameId,
            PlayerId = Guid.NewGuid(),
            UnitsToPlay = 1
        };
        
        // Act
        var result = command.Format(_localizationService, _game);
        
        // Assert
        result.ShouldBeEmpty();
    }
}