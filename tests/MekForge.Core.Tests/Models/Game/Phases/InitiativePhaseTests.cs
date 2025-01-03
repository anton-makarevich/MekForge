using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class InitiativePhaseTests : GameStateTestsBase
{
    private readonly InitiativePhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();

    public InitiativePhaseTests()
    {
        Game.IsAutoRoll = false;
        _sut = new InitiativePhase(Game);

        // Add two players
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));
    }

    private void SetupDiceRolls(params int[] rolls)
    {
        var callNumber = 0;
        DiceRoller.Roll2D6().Returns(_ =>
        {
            var currentRoll = rolls[callNumber % rolls.Length];
            callNumber++;
            return
            [
                new DiceResult { Result = currentRoll / 2 },
                new DiceResult { Result = (currentRoll + 1) / 2 }
            ];
        });
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
        SetupDiceRolls(7);

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
        SetupDiceRolls(8);
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = firstPlayer!.Id
        });

        // Second player rolls 6
        SetupDiceRolls(6);
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
        SetupDiceRolls(7);
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

    [Fact]
    public void Enter_WhenAutoRollEnabled_ShouldRollForAllPlayers()
    {
        // Arrange
        Game.IsAutoRoll = true;
        SetupDiceRolls(7,8); // First player rolls 7, Second player rolls 8
    
        // Act
        _sut.Enter();
    
        // Assert
        CommandPublisher.Received(2).PublishCommand(Arg.Any<DiceRolledCommand>());
        Game.TurnPhase.Should().Be(PhaseNames.Movement);
        Game.InitiativeOrder[0].Should().Be(Game.Players[1]); // Player with roll 8 should be first
        Game.InitiativeOrder[1].Should().Be(Game.Players[0]); // Player with roll 7 should be second
    }

    [Fact]
    public void Enter_WhenAutoRollAndTiesOccur_ShouldRerollAutomatically()
    {
        // Arrange
        Game.IsAutoRoll = true;
        SetupDiceRolls(7,7,8,6); // First player rolls 7
        // Second player rolls 7 too
        // First player rerolls 8
        // Second player rerolls 6
    
        // Act
        _sut.Enter();
    
        // Assert
        CommandPublisher.Received(4).PublishCommand(Arg.Any<DiceRolledCommand>()); // Should receive 4 roll commands (2 initial + 2 rerolls)
        Game.TurnPhase.Should().Be(PhaseNames.Movement); // Should proceed to movement after resolving ties
        Game.InitiativeOrder[0].Should().Be(Game.Players[0]); // Player who rerolled 8 should be first
        Game.InitiativeOrder[1].Should().Be(Game.Players[1]); // Player who rerolled 6 should be second
    }

    [Fact]
    public void Enter_WhenAutoRollAndMultipleTiesOccur_ShouldKeepRerollingUntilResolved()
    {
        // Arrange
        Game.IsAutoRoll = true;
        SetupDiceRolls(7,7,6,6,8,5); // First player rolls 7
        // Second player rolls 7
        // First player rerolls 6
        // Second player rerolls 6
        // First player rerolls again 8
        // Second player rerolls again 5
    
        // Act
        _sut.Enter();
    
        // Assert
        CommandPublisher.Received(6).PublishCommand(Arg.Any<DiceRolledCommand>()); // Should receive 6 roll commands (2 initial + 2 first reroll + 2 second reroll)
        Game.TurnPhase.Should().Be(PhaseNames.Movement);
        Game.InitiativeOrder[0].Should().Be(Game.Players[0]); // Player who rolled 8 should be first
        Game.InitiativeOrder[1].Should().Be(Game.Players[1]); // Player who rolled 5 should be second
    }

    [Fact]
    public void Enter_WhenAutoRollDisabled_ShouldWaitForPlayerCommands()
    {
        // Arrange
        Game.IsAutoRoll = false;
        Game.SetPhase(PhaseNames.Initiative);

        // Act
        _sut.Enter();

        // Assert
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<DiceRolledCommand>());
        Game.TurnPhase.Should().Be(PhaseNames.Initiative);
        Game.ActivePlayer.Should().Be(Game.Players[0]); // First player should be active
    }

    [Fact]
    public void HandleCommand_WhenManualRollingAndTies_ShouldHandleMultipleRounds()
    {
        // Arrange
        Game.IsAutoRoll = false;
        _sut.Enter();
        var player1 = Game.ActivePlayer;
        var player2 = Game.Players[1];

        // First round - both roll 7
        SetupDiceRolls(7, 7);

        // Player 1 rolls
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1!.Id
        });

        // Player 2 rolls
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });

        // Second round - 8 and 6
        SetupDiceRolls(8, 6);

        // Player 1 rolls in second round
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });

        // Player 2 rolls in second round
        _sut.HandleCommand(new RollDiceCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });

        // Assert
        CommandPublisher.Received(4).PublishCommand(Arg.Any<DiceRolledCommand>()); // Should receive 4 roll commands (2 initial + 2 rerolls)
        Game.TurnPhase.Should().Be(PhaseNames.Movement);
        Game.InitiativeOrder[0].Should().Be(player1); // Player who rolled 8 in second round should be first
        Game.InitiativeOrder[1].Should().Be(player2); // Player who rolled 6 in second round should be second
    }
}
