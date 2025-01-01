using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Server;

public class DiceRolledCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public DiceRolledCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new DiceRolledCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            Roll = 10,
        };

        _localizationService.GetString("Command_DiceRolled").Returns("formatted dice rolled command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted dice rolled command");
        _localizationService.Received(1).GetString("Command_DiceRolled");
    }
    
    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = new DiceRolledCommand
        {
            GameOriginId = _gameId,
            PlayerId = Guid.NewGuid(),
            Roll = 10,
        };

        // Act  
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
    }
}