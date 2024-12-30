using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Tests.Models.Game.States;
using Xunit;

namespace Sanet.MekForge.Core.Models.Game.States.Tests;

public class InitiativeStateTests : GameStateTestsBase
{
    private InitiativeState _sut = null!;
    private IDiceRoller _diceRoller = null!;

    private void ArrangeGame()
    {
        _diceRoller = Substitute.For<IDiceRoller>();
        SetupGame();
        _sut = new InitiativeState(Game);

        // Add two players
        Game.HandleCommand(CreateJoinCommand("player1", "Player 1"));
        Game.HandleCommand(CreateJoinCommand("player2", "Player 2"));
        Game.HandleCommand(CreateStatusCommand("player1", PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand("player2", PlayerStatus.Playing));
    }

    private void SetupDiceRoll(int total)
    {
        var roll1 = new DiceResult { Result = total / 2 };
        var roll2 = new DiceResult { Result = (total + 1) / 2 };
        _diceRoller.Roll2D6().Returns(new List<DiceResult> { roll1, roll2 });
    }

    [Fact]
    public void Name_ShouldBeInitiative()
    {
        ArrangeGame();
        _sut.Name.Should().Be("Initiative");
    }

    [Fact]
    public void Enter_ShouldSetFirstPlayerAsActive()
    {
        ArrangeGame();
        _sut.Enter();

        Game.ActivePlayer.Should().NotBeNull();
        VerifyActivePlayerChange(Game.ActivePlayer!.Id);
    }

    [Fact]
    public void HandleCommand_WhenPlayerRolls_ShouldPublishResult()
    {
        // Arrange
        ArrangeGame();
        _sut.Enter();
        SetupDiceRoll(7); // Total roll of 7

        // Act
        _sut.HandleCommand(new RollInitiativeCommand { PlayerId = Game.ActivePlayer!.Id });

        // Assert
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<InitiativeRolledCommand>(cmd =>
                cmd.PlayerId == Game.ActivePlayer.Id &&
                cmd.Roll == 7 &&
                cmd.GameOriginId == Game.GameId));
    }

    [Fact]
    public void HandleCommand_WhenAllPlayersRollDifferent_ShouldTransitionToMovement()
    {
        // Arrange
        ArrangeGame();
        _sut.Enter();
        var firstPlayer = Game.ActivePlayer;

        // First player rolls 8
        SetupDiceRoll(8);
        _sut.HandleCommand(new RollInitiativeCommand { PlayerId = firstPlayer!.Id });

        // Second player rolls 6
        SetupDiceRoll(6);
        _sut.HandleCommand(new RollInitiativeCommand 
        { 
            PlayerId = Game.Players.First(p => p != firstPlayer).Id 
        });

        // Assert
        Game.TurnPhase.Should().Be(Phase.Movement);
        Game.InitiativeOrder.Should().HaveCount(2);
        Game.InitiativeOrder[0].Should().Be(firstPlayer); // Higher roll should be first
    }

    [Fact]
    public void HandleCommand_WhenPlayersRollTie_ShouldRerollTiedPlayers()
    {
        // Arrange
        ArrangeGame();
        _sut.Enter();
        var firstPlayer = Game.ActivePlayer;
        var secondPlayer = Game.Players.First(p => p != firstPlayer);

        // Both players roll 7
        SetupDiceRoll(7);
        _sut.HandleCommand(new RollInitiativeCommand { PlayerId = firstPlayer!.Id });
        _sut.HandleCommand(new RollInitiativeCommand { PlayerId = secondPlayer.Id });

        // Clear previous command publications
        CommandPublisher.ClearReceivedCalls();

        // Assert
        Game.TurnPhase.Should().Be(Phase.Initiative); // Should stay in initiative
        Game.ActivePlayer.Should().BeOneOf(firstPlayer, secondPlayer); // One of tied players should be active
    }

    [Fact]
    public void HandleCommand_WhenWrongPlayerRolls_ShouldIgnoreCommand()
    {
        // Arrange
        ArrangeGame();
        _sut.Enter();
        var activePlayer = Game.ActivePlayer;

        // Act - wrong player tries to roll
        _sut.HandleCommand(new RollInitiativeCommand 
        { 
            PlayerId = Game.Players.First(p => p != activePlayer).Id 
        });

        // Assert
        Game.ActivePlayer.Should().Be(activePlayer); // Active player shouldn't change
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<InitiativeRolledCommand>());
    }
}
