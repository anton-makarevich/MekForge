using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class ServerGameTests
{
    private readonly ServerGame _serverGame;

    public ServerGameTests()
    {
        var battleState = new BattleState(new BattleMap(5, 5));
        var commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = Substitute.For<IRulesProvider>();
        _serverGame = new ServerGame(battleState, rulesProvider, commandPublisher);
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
        _serverGame.HandleCommand(joinCommand);

        // Assert
        _serverGame.Players.Should().HaveCount(1);
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
            GameOriginId = _serverGame.GameId // Set to this game's ID
        };

        // Act
        _serverGame.HandleCommand(command);

        // Assert
        // Verify that no players were added since the command was from this game instance
        _serverGame.Players.Should().BeEmpty();
    }
    
    [Fact]
    public void HandleCommand_ShouldProcessPlayerStatusCommand_WhenReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        
        _serverGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units=[]
        });

        var statusCommand = new PlayerStatusCommand
        {
            PlayerId = playerId,
            GameOriginId = Guid.NewGuid(),
            PlayerStatus = PlayerStatus.Playing
        };

        // Act
        _serverGame.HandleCommand(statusCommand);

        // Assert
        var updatedPlayer = _serverGame.Players.FirstOrDefault(p => p.Id == playerId);
        updatedPlayer.Should().NotBeNull();
        updatedPlayer.Status.Should().Be(PlayerStatus.Playing);
    }
}