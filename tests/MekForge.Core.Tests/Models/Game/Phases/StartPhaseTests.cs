using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class StartPhaseTests : GameStateTestsBase
{
    private StartPhase _sut = null!;

    [Fact]
    public void Name_ShouldBeStart()
    {
        _sut = new StartPhase(Game);

        _sut.Name.ShouldBe(PhaseNames.Start);
    }

    [Fact]
    public void HandleCommand_WhenPlayerJoins_ShouldAddPlayerToGame()
    {
        // Arrange
        _sut = new StartPhase(Game);
        var playerId = Guid.NewGuid();
        var joinCommand = CreateJoinCommand(playerId, "Player 1");

        // Act
        _sut.HandleCommand(joinCommand);

        // Assert
        Game.Players.Count.ShouldBe(1);
        Game.Players[0].Id.ShouldBe(playerId);
        Game.Players[0].Name.ShouldBe("Player 1");
        Game.Players[0].Units.Count.ShouldBe(1);
    }

    [Fact]
    public void HandleCommand_WhenAllPlayersReady_ShouldTransitionToDeployment()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        _sut = new StartPhase(Game);
        
        // Add two players
        _sut.HandleCommand(CreateJoinCommand(player1Id, "Player 1"));
        _sut.HandleCommand(CreateJoinCommand(player2Id, "Player 2"));

        // Act
        // Set both players ready
        _sut.HandleCommand(CreateStatusCommand(player1Id, PlayerStatus.Playing));
        _sut.HandleCommand(CreateStatusCommand(player2Id, PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.ShouldBe(PhaseNames.Deployment);
        VerifyPhaseChange(PhaseNames.Deployment);
        
        // Should set first player as active
        Game.ActivePlayer.ShouldNotBeNull();
        VerifyActivePlayerChange(Game.ActivePlayer?.Id);
    }

    [Fact]
    public void HandleCommand_WhenNotAllPlayersReady_ShouldStayInStartPhase()
    {
        // Arrange
        _sut = new StartPhase(Game);
        
        // Add two players
        _sut.HandleCommand(CreateJoinCommand(Guid.NewGuid(), "Player 1"));
        _sut.HandleCommand(CreateJoinCommand(Guid.NewGuid(), "Player 2"));

        // Act
        // Set only one player ready
        _sut.HandleCommand(CreateStatusCommand(Guid.NewGuid(), PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.ShouldBe(PhaseNames.Start);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangePhaseCommand>());
        Game.ActivePlayer.ShouldBeNull();
    }

    [Fact]
    public void HandleCommand_WhenNoPlayers_ShouldStayInStartPhase()
    {
        // Arrange
        _sut = new StartPhase(Game);

        // Act
        _sut.HandleCommand(CreateStatusCommand(Guid.NewGuid(), PlayerStatus.Playing));

        // Assert
        Game.TurnPhase.ShouldBe(PhaseNames.Start);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangePhaseCommand>());
        Game.ActivePlayer.ShouldBeNull();
    }
}
