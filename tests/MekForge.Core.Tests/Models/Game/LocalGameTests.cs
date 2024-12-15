using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class LocalGameTests
{
    private readonly LocalGame _localGame;
    private readonly ICommandPublisher _commandPublisher;

    public LocalGameTests()
    {
        var battleState = new BattleState(new BattleMap(5, 5));
        _commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = Substitute.For<IRulesProvider>();
        _localGame = new LocalGame(battleState, rulesProvider, _commandPublisher);
    }

    [Fact]
    public void HandleCommand_ShouldAddPlayer_WhenJoinGameCommandIsReceived()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = new List<UnitData>()
        };

        // Act
        _localGame.HandleCommand(joinCommand);

        // Assert
        _localGame.Players.Should().HaveCount(1);
        _localGame.Players[0].Name.Should().Be(joinCommand.PlayerName);
    }
    
    [Fact]
    public void HandleCommand_ShouldNotProcessOwnCommands_WhenGameOriginIdMatches()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = new List<UnitData>(),
            GameOriginId = _localGame.GameId // Set to this game's ID
        };

        // Act
        _localGame.HandleCommand(command);

        // Assert
        // Verify that no players were added since the command was from this game instance
        _localGame.Players.Should().BeEmpty();
    }

    [Fact]
    public void JoinGameWithUnits_ShouldPublishJoinGameCommand_WhenCalled()
    {
        // Arrange
        var units = new List<UnitData>();
        var player = new Player(Guid.NewGuid(), "Player1");

        // Act
        _localGame.JoinGameWithUnits(player, units);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<JoinGameCommand>(cmd =>
            cmd.PlayerId == player.Id &&
            cmd.PlayerName == player.Name &&
            cmd.Units.Count == units.Count));
    }
}