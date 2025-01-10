using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game;

public class ClientGameTests
{
    private readonly ClientGame _clientGame;
    private readonly ICommandPublisher _commandPublisher;

    public ClientGameTests()
    {
        var battleState = BattleMap.GenerateMap(5, 5, new SingleTerrainGenerator(5,5, new ClearTerrain()));
        _commandPublisher = Substitute.For<ICommandPublisher>();
        var rulesProvider = Substitute.For<IRulesProvider>();
        _clientGame = new ClientGame(battleState,[], rulesProvider, _commandPublisher);
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
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        _clientGame.Players.Should().HaveCount(1);
        _clientGame.Players[0].Name.Should().Be(joinCommand.PlayerName);
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
            GameOriginId = _clientGame.Id, // Set to this game's ID
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(command);

        // Assert
        // Verify that no players were added since the command was from this game instance
        _clientGame.Players.Should().BeEmpty();
    }

    [Fact]
    public void JoinGameWithUnits_ShouldPublishJoinGameCommand_WhenCalled()
    {
        // Arrange
        var units = new List<UnitData>();
        var player = new Player(Guid.NewGuid(), "Player1");

        // Act
        _clientGame.JoinGameWithUnits(player, units);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<JoinGameCommand>(cmd =>
            cmd.PlayerId == player.Id &&
            cmd.PlayerName == player.Name &&
            cmd.Units.Count == units.Count));
    }
    
    [Fact]
    public void HandleCommand_ShouldSetPlayerStatus_WhenPlayerStatusCommandIsReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name, Units = [],
            Tint = "#FF0000"
        });

        var statusCommand = new UpdatePlayerStatusCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerStatus = PlayerStatus.Playing
        };

        // Act
        _clientGame.HandleCommand(statusCommand);

        // Assert
        var updatedPlayer = _clientGame.Players.FirstOrDefault(p => p.Id == playerId);
        updatedPlayer.Should().NotBeNull();
        updatedPlayer.Status.Should().Be(PlayerStatus.Playing);
    }
    
    [Fact]
    public void SetPlayerReady_ShouldNotPublishPlayerStatusCommand_WhenCalled_ButPlayerIsNotInGame()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");

        // Act
        _clientGame.SetPlayerReady(player);

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<UpdatePlayerStatusCommand>());
    }
    
    [Fact]
    public void SetPlayerReady_ShouldPublishPlayerStatusCommand_WhenCalled_AndPlayerIsInGame()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name, Units = [],
            Tint = "#FF0000"
        });

        // Act
        _clientGame.SetPlayerReady(player);

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<UpdatePlayerStatusCommand>(cmd => 
            cmd.PlayerId == player.Id && 
            cmd.PlayerStatus == PlayerStatus.Playing &&
            cmd.GameOriginId == _clientGame.Id
        ));
    }

    [Fact]
    public void ChangePhase_ShouldProcessCommand()
    {
        // Arrange
        var command = new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.End
        };
        
        // Act
        _clientGame.HandleCommand(command);
        
        // Assert
        _clientGame.TurnPhase.Should().Be(PhaseNames.End);
    }
    
    [Fact]
    public void ChangeActivePlayer_ShouldProcessCommand()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            PlayerName = player.Name, Units = [],
            Tint = "#FF0000"
        });
        var actualPlayer = _clientGame.Players.FirstOrDefault(p => p.Id == player.Id);
        var command = new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 0
        };
        
        // Act
        _clientGame.HandleCommand(command);
        
        // Assert
        _clientGame.ActivePlayer.Should().Be(actualPlayer);
        actualPlayer.Name.Should().Be(player.Name);
        actualPlayer.Id.Should().Be(player.Id);
    }

    [Fact]
    public void HandleCommand_ShouldAddCommandToLog_WhenCommandIsValid()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        _clientGame.CommandLog.Should().HaveCount(1);
        _clientGame.CommandLog[0].Should().BeEquivalentTo(joinCommand);
    }

    [Fact]
    public void HandleCommand_ShouldNotAddCommandToLog_WhenGameOriginIdMatches()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            Units = [],
            GameOriginId = _clientGame.Id,
            Tint = "#FF0000"
        };

        // Act
        _clientGame.HandleCommand(command);

        // Assert
        _clientGame.CommandLog.Should().BeEmpty();
    }

    [Fact]
    public void Commands_ShouldEmitCommand_WhenHandleCommandIsCalled()
    {
        // Arrange
        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player1",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };
        var receivedCommands = new List<GameCommand>();
        using var subscription = _clientGame.Commands.Subscribe(cmd => receivedCommands.Add(cmd));

        // Act
        _clientGame.HandleCommand(joinCommand);

        // Assert
        receivedCommands.Should().HaveCount(1);
        receivedCommands.First().Should().BeEquivalentTo(joinCommand);
    }
}