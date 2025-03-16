using NSubstitute;
using Sanet.MekForge.Core.Models.Game;
using Shouldly;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class PhysicalAttackPhaseTests : GamePhaseTestsBase
{
    private readonly PhysicalAttackPhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();
    private readonly Guid _unit1Id;
    private readonly Guid _unit2Id;

    public PhysicalAttackPhaseTests()
    {
        _sut = new PhysicalAttackPhase(Game);

        // Add two players with units
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1", 2));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));
        
        // Get unit IDs
        var player1 = Game.Players[0];
        _unit1Id = player1.Units[0].Id;

        var player2 = Game.Players[1];
        _unit2Id = player2.Units[0].Id;

        // Set initiative order (player2 won, player1 lost)
        Game.SetInitiativeOrder(new List<IPlayer> { player2, player1 });

        // Deploy units
        foreach (var unit in player1.Units.Concat(player2.Units))
        {
            unit.Deploy(new HexPosition(1, 1, HexDirection.Top));
        }
    }

    [Fact]
    public void Enter_ShouldSetFirstPlayerActive()
    {
        // Act
        _sut.Enter();
    
        // Assert
        Game.ActivePlayer.ShouldBe(Game.Players[0]); // Player who lost initiative attacks first
    }

    [Fact]
    public void HandleCommand_WhenValidPhysicalAttack_ShouldPublishAndUpdateTurn()
    {
        // Arrange
        _sut.Enter();
        
        // Act
        _sut.HandleCommand(new PhysicalAttackCommand
        {
            GameOriginId = Game.Id,
            PlayerId = Game.ActivePlayer!.Id,
            AttackerUnitId = _unit1Id,
            TargetUnitId = _unit2Id,
            AttackType = PhysicalAttackType.Punch
        });
    
        // Assert
        CommandPublisher.Received(1).PublishCommand(Arg.Is<PhysicalAttackCommand>(cmd => 
            cmd.AttackerUnitId == _unit1Id && 
            cmd.TargetUnitId == _unit2Id &&
            cmd.AttackType == PhysicalAttackType.Punch));
    }

    [Fact]
    public void HandleCommand_WhenWrongPlayer_ShouldIgnoreCommand()
    {
        // Arrange
        _sut.Enter();
        var wrongPlayerId = Guid.NewGuid();
    
        // Act
        _sut.HandleCommand(new PhysicalAttackCommand
        {
            GameOriginId = Game.Id,
            PlayerId = wrongPlayerId,
            AttackerUnitId = _unit1Id,
            TargetUnitId = _unit2Id,
            AttackType = PhysicalAttackType.Kick
        });
    
        // Assert
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<PhysicalAttackCommand>());
    }

    [Fact]
    public void HandleCommand_WhenAllUnitsAttacked_ShouldTransitionToEndPhase()
    {
        // Arrange
        _sut.Enter();
        var firstPlayer = Game.ActivePlayer!;
        
        // Act - First player attacks with all units
        foreach (var unit in firstPlayer.Units)
        {
            _sut.HandleCommand(new PhysicalAttackCommand
            {
                GameOriginId = Game.Id,
                PlayerId = firstPlayer.Id,
                AttackerUnitId = unit.Id,
                TargetUnitId = _unit2Id,
                AttackType = PhysicalAttackType.Punch
            });
        }

        // Second player should be active now
        Game.ActivePlayer.ShouldNotBe(firstPlayer);
        var secondPlayer = Game.ActivePlayer;
        
        // Second player attacks
        foreach (var unit in secondPlayer!.Units)
        {
            _sut.HandleCommand(new PhysicalAttackCommand
            {
                GameOriginId = Game.Id,
                PlayerId = secondPlayer.Id,
                AttackerUnitId = unit.Id,
                TargetUnitId = _unit1Id,
                AttackType = PhysicalAttackType.Punch
            });
        }
    
        // Assert
        Game.TurnPhase.ShouldBe(PhaseNames.End);
    }

    [Fact]
    public void HandleCommand_WhenInvalidCommand_ShouldIgnoreCommand()
    {
        // Arrange
        Game.TransitionToPhase(new PhysicalAttackPhase(Game));
        _sut.Enter();
        
        // Act
        _sut.HandleCommand(new WeaponAttackDeclarationCommand // Wrong command type
        {
            GameOriginId = Game.Id,
            PlayerId = Game.ActivePlayer!.Id,
            AttackerId = _unit1Id,
            WeaponTargets = []
        });
    
        // Assert
        CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<WeaponAttackDeclarationCommand>());
        Game.TurnPhase.ShouldBe(PhaseNames.PhysicalAttack);
    }
}
