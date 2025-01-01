using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class RollDiceCommadTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public RollDiceCommadTests()
    {
        _game.Players.Returns([_player1]);
    }
    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new RollDiceCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
        };
        
        _localizationService.GetString("Command_RollDice").Returns("formatted roll command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted roll command");
    }

    [Fact]
    public void Format_ShouldReturnEmpty_WhenPlayerNotFound()
    {
        // Arrange
        var command = new RollDiceCommand
        {
            GameOriginId = _gameId,
            PlayerId = Guid.NewGuid(),
        };

        _localizationService.GetString("Command_RollDice").Returns("formatted roll command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().BeEmpty();
    }
}