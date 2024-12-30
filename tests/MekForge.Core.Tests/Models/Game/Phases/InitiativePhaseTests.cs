using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.States;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class InitiativePhaseTests : GameStateTestsBase
{
    private readonly InitiativePhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();

    public InitiativePhaseTests()
    {
        _sut = new InitiativePhase(Game);

        // Add two players
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));
    }

    private void SetupDiceRoll(int total)
    {
        var roll1 = new DiceResult { Result = total / 2 };
        var roll2 = new DiceResult { Result = (total + 1) / 2 };
        DiceRoller.Roll2D6().Returns([roll1, roll2]);
    }

    [Fact]
    public void Name_ShouldBeInitiative()
    {
        _sut.Name.Should().Be(PhaseNames.Initiative);
    }

    [Fact]
    public void Enter_ShouldSetFirstPlayerAsActive()
    {
        _sut.Enter();

        Game.ActivePlayer.Should().NotBeNull();
    }

    [Fact]
    public void HandleCommand_WhenPlayerRolls_ShouldPublishResult()
    {
        // Arrange
        _sut.Enter();
        SetupDiceRoll(7); // Total roll of 7

        // Act
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = Game.ActivePlayer!.Id
        });

        // Assert
        CommandPublisher.Received().PublishCommand(Arg.Do<DiceRolledCommand>(cmd =>
        {
            cmd.GameOriginId.Should().Be(Game.GameId);
            cmd.PlayerId.Should().Be(Game.ActivePlayer!.Id);
            cmd.Roll.Should().Be(7);
        }));
    }

    [Fact]
    public void HandleCommand_WhenAllPlayersRollDifferent_ShouldTransitionToMovement()
    {
        // Arrange
        _sut.Enter();
        var firstPlayer = Game.ActivePlayer;

        // First player rolls 8
        SetupDiceRoll(8);
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = firstPlayer!.Id
        });

        // Second player rolls 6
        SetupDiceRoll(6);
        _sut.HandleCommand(new RollDiceCommand 
        { 
            GameOriginId = Guid.NewGuid(),
            PlayerId = Game.Players.First(p => p != firstPlayer).Id 
        });

        // Assert
        Game.TurnPhase.Should().Be(PhaseNames.Movement);
        Game.InitiativeOrder.Should().HaveCount(2);
        Game.InitiativeOrder[0].Should().Be(firstPlayer); // Higher roll should be first
    }

    [Fact]
    public void HandleCommand_WhenPlayersRollTie_ShouldRerollTiedPlayers()
    {
        // Arrange
        Game.SetPhase(PhaseNames.Initiative);
        _sut.Enter();
        var firstPlayer = Game.ActivePlayer;
        var secondPlayer = Game.Players.First(p => p != firstPlayer);

        // Both players roll 7
        SetupDiceRoll(7);
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = firstPlayer!.Id
        });
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = secondPlayer.Id
        });

        // Clear previous command publications
        CommandPublisher.ClearReceivedCalls();

        // Assert
        Game.TurnPhase.Should().Be(PhaseNames.Initiative); // Should stay in initiative
        Game.ActivePlayer.Should().BeOneOf(firstPlayer, secondPlayer); // One of tied players should be active
    }

    [Fact]
    public void HandleCommand_WhenWrongPlayerRolls_ShouldIgnoreCommand()
    {
        // Arrange
        _sut.Enter();
        var activePlayer = Game.ActivePlayer;

        // Act - wrong player tries to roll
        _sut.HandleCommand(new RollDiceCommand 
        { 
            GameOriginId = Guid.NewGuid(),
            PlayerId = Game.Players.First(p => p != activePlayer).Id 
        });

        // Assert
        Game.ActivePlayer.Should().Be(activePlayer); // Active player shouldn't change
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<DiceRolledCommand>());
    }
}
