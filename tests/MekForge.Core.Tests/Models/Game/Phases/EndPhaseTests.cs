using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Units;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class EndPhaseTests : GamePhaseTestsBase
{
    private readonly EndPhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();
    private readonly Guid _player3Id = Guid.NewGuid();

    public EndPhaseTests()
    {
        // Add three players
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateJoinCommand(_player3Id, "Player 3"));
        
        // Set all players to Playing status
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player3Id, PlayerStatus.Playing));
        
        // Set initiative order
        var players = Game.Players.ToList();
        Game.SetInitiativeOrder(new List<IPlayer> { players[0], players[1], players[2] });
        
        // Clear any commands published during setup
        CommandPublisher.ClearReceivedCalls();
        
        // Create the EndPhase
        _sut = new EndPhase(Game);
    }

    [Fact]
    public void Enter_ShouldSetFirstPlayerInInitiativeOrderAsActive()
    {
        // Arrange
        CommandPublisher.ClearReceivedCalls();
        
        // Act
        _sut.Enter();
        
        // Assert
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player1Id);
        VerifyActivePlayerChange(_player1Id);
    }
    
    [Fact]
    public void HandleCommand_ShouldSetNextPlayerAsActive_WhenPlayerEndsTurn()
    {
        // Arrange
        _sut.Enter();
        CommandPublisher.ClearReceivedCalls();
        
        // Act
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player2Id);
        VerifyActivePlayerChange(_player2Id);
    }
    
    [Fact]
    public void HandleCommand_ShouldSetThirdPlayerAsActive_WhenFirstAndSecondPlayersEndTurn()
    {
        // Arrange
        _sut.Enter();
        
        // First player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Second player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player2Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player3Id);
        VerifyActivePlayerChange(_player3Id);
    }
    
    [Fact]
    public void HandleCommand_ShouldIncrementTurnAndTransitionToInitiativePhase_WhenAllPlayersEndTurn()
    {
        // Arrange
        _sut.Enter();
        var initialTurn = Game.Turn;
        
        // All players end their turns
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player2Id,
            Timestamp = DateTime.UtcNow
        });
        
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Last player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player3Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert
        Game.Turn.ShouldBe(initialTurn + 1);
        VerifyPhaseChange(PhaseNames.Initiative);
        
        // Verify TurnIncrementedCommand was published
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<TurnIncrementedCommand>(cmd => 
                cmd.TurnNumber == initialTurn + 1 && 
                cmd.GameOriginId == Game.Id));
    }
    
    [Fact]
    public void HandleCommand_ShouldIgnoreNonTurnEndedCommands()
    {
        // Arrange
        _sut.Enter();
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Send a different command type
        _sut.HandleCommand(new MoveUnitCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            UnitId = Guid.NewGuid(),
            MovementType = MovementType.Walk,
            MovementPath = []
        });
        
        // Assert - No changes should occur
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player1Id);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangeActivePlayerCommand>());
    }
    
    [Fact]
    public void HandleCommand_ShouldHandlePlayersEndingTurnInDifferentOrder()
    {
        // Arrange
        _sut.Enter();
        var initialTurn = Game.Turn;
        
        // Players end turns in reverse initiative order
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player3Id,
            Timestamp = DateTime.UtcNow
        });
        
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player2Id,
            Timestamp = DateTime.UtcNow
        });
        
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Last player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert
        Game.Turn.ShouldBe(initialTurn + 1);
        VerifyPhaseChange(PhaseNames.Initiative);
    }
}
