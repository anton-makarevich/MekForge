﻿using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands.Client;

public class JoinGameCommandTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly IGame _game = Substitute.For<IGame>();
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Player _player1 = new Player(Guid.NewGuid(), "Player 1");

    public JoinGameCommandTests()
    {
        _game.Players.Returns([_player1]);
    }

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameOriginId = _gameId,
            PlayerId = _player1.Id,
            PlayerName = _player1.Name,
            Units = []
        };

        _localizationService.GetString("Command_JoinGame").Returns("formatted join command");

        // Act
        var result = command.Format(_localizationService, _game);

        // Assert
        result.Should().Be("formatted join command");
        _localizationService.Received(1).GetString("Command_JoinGame");
    }
    
}