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
    private readonly IGamePhase _mockNextPhase;

    public EndPhaseTests()
    {
        // Create mock next phase and configure the phase manager
        _mockNextPhase = Substitute.For<IGamePhase>();
        MockPhaseManager.GetNextPhase(PhaseNames.End, Game).Returns(_mockNextPhase);
        
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
    public void HandleCommand_ShouldSetNextPlayerAsActive_WhenActivePlayerEndsTurn()
    {
        // Arrange
        _sut.Enter();
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Active player (player1) ends turn
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
    public void HandleCommand_ShouldIgnoreCommands_FromNonActivePlayer()
    {
        // Arrange
        _sut.Enter();
        CommandPublisher.ClearReceivedCalls();
        
        // Act - Non-active player (player2) tries to end turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player2Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert - Active player should still be player1
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player1Id);
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<ChangeActivePlayerCommand>());
    }
    
    [Fact]
    public void HandleCommand_ShouldProgressThroughAllPlayers_InInitiativeOrder()
    {
        // Arrange
        _sut.Enter();
        
        // Act & Assert - First player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player2Id);
        
        // Act & Assert - Second player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player2Id,
            Timestamp = DateTime.UtcNow
        });
        
        Game.ActivePlayer.ShouldNotBeNull();
        Game.ActivePlayer.Id.ShouldBe(_player3Id);
    }
    
    [Fact]
    public void HandleCommand_ShouldIncrementTurnAndTransitionToNextPhase_WhenAllPlayersEndTurn()
    {
        // Arrange
        _sut.Enter();
        var initialTurn = Game.Turn;
        
        // First player ends turn
        _sut.HandleCommand(new TurnEndedCommand
        {
            GameOriginId = Game.Id,
            PlayerId = _player1Id,
            Timestamp = DateTime.UtcNow
        });
        
        // Second player ends turn
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
        
        // Verify the phase manager was called to get the next phase
        MockPhaseManager.Received(1).GetNextPhase(PhaseNames.End, Game);
        
        // Verify the mock next phase was entered
        _mockNextPhase.Received(1).Enter();
        
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
}
