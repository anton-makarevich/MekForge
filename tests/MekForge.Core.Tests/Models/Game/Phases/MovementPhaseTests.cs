using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class MovementPhaseTests : GameStateTestsBase
{
    private readonly MovementPhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();
    private readonly Guid _unit1Id = Guid.NewGuid();
    private readonly Guid _unit2Id = Guid.NewGuid();
    private readonly Guid _unit3Id = Guid.NewGuid();
    
    private readonly MechFactory _mechFactory = new MechFactory(new ClassicBattletechRulesProvider());

    public MovementPhaseTests()
    {
        _sut = new MovementPhase(Game);

        // Add two players with units
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));

        // Add units to players
        var player1 = Game.Players[0];
        player1.Units.Returns(new List<Unit> 
        { 
            CreateUnit(_unit1Id),
            CreateUnit(_unit2Id)
        });

        var player2 = Game.Players[1];
        player2.Units.Returns(new List<Unit> 
        { 
            CreateUnit(_unit3Id)
        });

        // Set initiative order (player2 won, player1 lost)
        Game.SetInitiativeOrder(new List<IPlayer> { player2, player1 });
    }

    private Unit CreateUnit(Guid id)
    {
     var unitData = MechFactoryTests.CreateDummyMechData();
        unitData.Id= id;
        return _mechFactory.Create(unitData);
    }

    // [Fact]
    // public void Enter_ShouldSetFirstPlayerActive()
    // {
    //     // Act
    //     _sut.Enter();
    //
    //     // Assert
    //     Game.ActivePlayer.Should().Be(Game.Players[1]); // Player who lost initiative moves first
    // }

    // [Fact]
    // public void HandleCommand_WhenValidMove_ShouldPublishAndUpdateTurn()
    // {
    //     // Arrange
    //     _sut.Enter();
    //     var newPosition = new Position(1, 1);
    //
    //     // Act
    //     _sut.HandleCommand(new MoveUnitCommand
    //     {
    //         GameOriginId = Game.GameId,
    //         PlayerId = Game.ActivePlayer!.Id,
    //         UnitId = _unit1Id,
    //         NewPosition = newPosition
    //     });
    //
    //     // Assert
    //     CommandPublisher.Received(1).PublishCommand(Arg.Is<UnitMovedCommand>(cmd =>
    //         cmd.GameOriginId == Game.GameId &&
    //         cmd.PlayerId == Game.ActivePlayer.Id &&
    //         cmd.UnitId == _unit1Id &&
    //         cmd.NewPosition == newPosition));
    // }

    // [Fact]
    // public void HandleCommand_WhenWrongPlayer_ShouldIgnoreCommand()
    // {
    //     // Arrange
    //     _sut.Enter();
    //     var wrongPlayerId = Guid.NewGuid();
    //
    //     // Act
    //     _sut.HandleCommand(new MoveUnitCommand
    //     {
    //         GameOriginId = Game.GameId,
    //         PlayerId = wrongPlayerId,
    //         UnitId = _unit1Id,
    //         NewPosition = new Position(1, 1)
    //     });
    //
    //     // Assert
    //     CommandPublisher.DidNotReceive().PublishCommand(Arg.Any<UnitMovedCommand>());
    // }

    // [Fact]
    // public void HandleCommand_WhenAllUnitsMoved_ShouldTransitionToAttack()
    // {
    //     // Arrange
    //     _sut.Enter();
    //
    //     // Move all units
    //     foreach (var player in Game.Players)
    //     {
    //         foreach (var unit in player.Units)
    //         {
    //             _sut.HandleCommand(new MoveUnitCommand
    //             {
    //                 GameOriginId = Game.GameId,
    //                 PlayerId = Game.ActivePlayer!.Id,
    //                 UnitId = unit.Id,
    //                 Destination= new HexCoordinateData(1, 1)
    //             });
    //         }
    //     }
    //
    //     // Assert
    //     Game.TurnPhase.Should().Be(PhaseNames.Attack);
    // }
}
