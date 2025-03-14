using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Tests.Data.Community;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class DeploymentPhaseTests : GameStateTestsBase
{
    private DeploymentPhase _sut = null!;

    [Fact]
    public void Name_ShouldBeDeployment()
    {
        _sut = new DeploymentPhase(Game);

        _sut.Name.ShouldBe(PhaseNames.Deployment);
    }

    [Fact]
    public void Enter_ShouldSetRandomPlayerAsActive()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        _sut = new DeploymentPhase(Game);

        // Add two players
        Game.HandleCommand(CreateJoinCommand(player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(player2Id, PlayerStatus.Playing));

        // Act
        _sut.Enter();

        // Assert
        Game.ActivePlayer.ShouldNotBeNull();
        var playerNames = Game.Players.Select(p => p.Name).ToArray();
        playerNames.ShouldContain(Game.ActivePlayer.Name);
    }

    [Fact]
    public void HandleCommand_WhenUnitDeployed_ShouldUpdateUnitPosition()
    {
        // Arrange
        Game.IsAutoRoll = false;
        var playerId = Guid.NewGuid();
        _sut = new DeploymentPhase(Game);

        // Add a player with a unit
        Game.HandleCommand(CreateJoinCommand(playerId, "Player 1"));
        Game.HandleCommand(CreateStatusCommand(playerId, PlayerStatus.Playing));
        _sut.Enter();

        // Act
        var player = Game.Players[0];
        _sut.HandleCommand(CreateDeployCommand(playerId, player.Units[0].Id, 1, 1, 0));

        // Assert
        var unit = player.Units[0];
        unit.IsDeployed.ShouldBeTrue();
        unit.Position.ShouldNotBeNull();
        unit.Position.Coordinates.Q.ShouldBe(1);
        unit.Position.Coordinates.R.ShouldBe(1);
    }

    [Fact]
    public void HandleCommand_WhenAllUnitsDeployed_ShouldTransitionToInitiative()
    {
        // Arrange
        Game.IsAutoRoll = false;
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        _sut = new DeploymentPhase(Game);

        // Add two players with one unit each
        Game.HandleCommand(CreateJoinCommand(player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(player2Id, PlayerStatus.Playing));
        _sut.Enter();

        // Deploy first player's unit
        _sut.HandleCommand(CreateDeployCommand(Game.ActivePlayer!.Id, Game.ActivePlayer.Units[0].Id, 1, 1, 0));

        // Act
        // Deploy second player's unit
        _sut.HandleCommand(CreateDeployCommand(Game.ActivePlayer.Id, Game.ActivePlayer.Units[0].Id, 2, 2, 0));
        
        // Assert
        Game.TurnPhase.ShouldBe(PhaseNames.Initiative);
        VerifyPhaseChange(PhaseNames.Initiative);
    }

    [Fact]
    public void HandleCommand_WhenActivePlayerHasMoreUnits_ShouldKeepSameActivePlayer()
    {
        // Arrange
       var playerId = Guid.NewGuid();
       var unit1 = MechFactoryTests.CreateDummyMechData();
       unit1.Id = Guid.NewGuid();
       var unit2 = MechFactoryTests.CreateDummyMechData();
       unit2.Id = Guid.NewGuid();
        _sut = new DeploymentPhase(Game);

        // Add a player with two units
        var joinCommand = new JoinGameCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            PlayerName = "Player 1",
            Units = [unit1, unit2],
            Tint = "#FF0000"
        };
        Game.HandleCommand(joinCommand);
        Game.HandleCommand(CreateStatusCommand(playerId, PlayerStatus.Playing));
        _sut.Enter();

        var initialActivePlayer = Game.ActivePlayer;
        
        // Clear previous command publications
        CommandPublisher.ClearReceivedCalls();

        // Act
        _sut.HandleCommand(CreateDeployCommand(playerId, unit1.Id.Value, 1, 1, 0));

        // Assert
        Game.ActivePlayer.ShouldBe(initialActivePlayer);
        Game.TurnPhase.ShouldBe(PhaseNames.Deployment);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangePhaseCommand>());
    }

    [Fact]
    public void HandleCommand_WhenActivePlayerFinishesDeployment_ShouldSwitchToNextPlayer()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        _sut = new DeploymentPhase(Game);

        // Add two players
        Game.HandleCommand(CreateJoinCommand(player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(player2Id, PlayerStatus.Playing));
        _sut.Enter();

        var initialActivePlayer = Game.ActivePlayer;

        // Clear previous command publications
        CommandPublisher.ClearReceivedCalls();

        // Act
        _sut.HandleCommand(CreateDeployCommand(initialActivePlayer!.Id, initialActivePlayer.Units[0].Id, 1, 1, 0));

        // Assert
        Game.ActivePlayer.ShouldNotBe(initialActivePlayer);
        Game.TurnPhase.ShouldBe(PhaseNames.Deployment);
    }
}
