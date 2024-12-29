using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.States;

namespace Sanet.MekForge.Core.Tests.Models.Game.States;

public class StartStateTests : GameStateTestsBase
{
    private StartState _sut = null!;

    [Fact]
    public void Name_ShouldBeStart()
    {
        _sut = new StartState(Game);

        _sut.Name.Should().Be("Start");
    }

    [Fact]
    public void HandleCommand_WhenPlayerJoins_ShouldAddPlayerToGame()
    {
        // Arrange
        _sut = new StartState(Game);
        var playerId = Guid.NewGuid();
        var joinCommand = CreateJoinCommand(playerId, "Player 1");

        // Act
        _sut.HandleCommand(joinCommand);

        // Assert
        Game.Players.Should().HaveCount(1);
        Game.Players[0].Id.Should().Be(playerId);
        Game.Players[0].Name.Should().Be("Player 1");
        Game.Players[0].Units.Should().HaveCount(1);
    }

    [Fact]
    public void HandleCommand_WhenAllPlayersReady_ShouldTransitionToDeployment()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        _sut = new StartState(Game);
        
        // Add two players
        _sut.HandleCommand(CreateJoinCommand(player1Id, "Player 1"));
        _sut.HandleCommand(CreateJoinCommand(player2Id, "Player 2"));

        // Act
        // Set both players ready
        _sut.HandleCommand(CreateStatusCommand(player1Id, PlayerStatus.Playing));
        _sut.HandleCommand(CreateStatusCommand(player2Id, PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.Should().Be(Phase.Deployment);
        VerifyPhaseChange(Phase.Deployment);
        
        // Should set first player as active
        Game.ActivePlayer.Should().NotBeNull();
        VerifyActivePlayerChange(Game.ActivePlayer?.Id);
    }

    [Fact]
    public void HandleCommand_WhenNotAllPlayersReady_ShouldStayInStartPhase()
    {
        // Arrange
        _sut = new StartState(Game);
        
        // Add two players
        _sut.HandleCommand(CreateJoinCommand(Guid.NewGuid(), "Player 1"));
        _sut.HandleCommand(CreateJoinCommand(Guid.NewGuid(), "Player 2"));

        // Act
        // Set only one player ready
        _sut.HandleCommand(CreateStatusCommand(Guid.NewGuid(), PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.Should().Be(Phase.Start);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangePhaseCommand>());
        Game.ActivePlayer.Should().BeNull();
    }

    [Fact]
    public void HandleCommand_WhenNoPlayers_ShouldStayInStartPhase()
    {
        // Arrange
        _sut = new StartState(Game);

        // Act
        _sut.HandleCommand(CreateStatusCommand(Guid.NewGuid(), PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.Should().Be(Phase.Start);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangePhaseCommand>());
        Game.ActivePlayer.Should().BeNull();
    }
}
