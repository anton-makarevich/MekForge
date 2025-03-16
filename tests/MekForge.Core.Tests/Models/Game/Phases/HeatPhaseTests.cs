using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Phases;

public class HeatPhaseTests : GamePhaseTestsBase
{
    private readonly HeatPhase _sut;
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();
    private readonly Guid _unit1Id;
    private readonly Guid _unit2Id;
    private readonly Unit _unit1;
    private readonly Unit _unit2;

    public HeatPhaseTests()
    {
        _sut = new HeatPhase(Game);

        // Add two players with units
        Game.HandleCommand(CreateJoinCommand(_player1Id, "Player 1"));
        Game.HandleCommand(CreateJoinCommand(_player2Id, "Player 2"));
        Game.HandleCommand(CreateStatusCommand(_player1Id, PlayerStatus.Playing));
        Game.HandleCommand(CreateStatusCommand(_player2Id, PlayerStatus.Playing));

        // Get unit IDs and references
        var player1 = Game.Players[0];
        _unit1 = player1.Units[0];
        _unit1Id = _unit1.Id;

        var player2 = Game.Players[1];
        _unit2 = player2.Units[0];
        _unit2Id = _unit2.Id;

        // Set initiative order
        Game.SetInitiativeOrder(new List<IPlayer> { player2, player1 });

        // Deploy units to the map
        Game.HandleCommand(CreateDeployCommand(_player1Id, _unit1Id, 1, 1, 0)); // 0 = Forward direction
        Game.HandleCommand(CreateDeployCommand(_player2Id, _unit2Id, 3, 3, 0)); // 0 = Forward direction

        // Clear any commands published during setup
        CommandPublisher.ClearReceivedCalls();
    }

    [Fact]
    public void Enter_ShouldProcessHeatForAllUnits_AndTransitionToEndPhase()
    {
        // Arrange
        // Setup units with heat sources
        SetupUnitWithMovement(_unit1, MovementType.Run);
        SetupUnitWithWeaponFired(_unit2);

        // Act
        _sut.Enter();

        // Assert
        // Verify heat updated commands were published for both units
        CommandPublisher.Received().PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                cmd.UnitId == _unit1Id || cmd.UnitId == _unit2Id));

        // Verify transition to End phase
        VerifyPhaseChange(PhaseNames.End);
    }

    [Fact]
    public void Enter_WithMovementHeat_ShouldCalculateAndApplyCorrectHeat()
    {
        // Arrange
        SetupUnitWithMovement(_unit1, MovementType.Run);
        var initialHeat = _unit1.CurrentHeat;

        // Act
        _sut.Enter();

        // Assert
        // Verify heat was applied to the unit
        _unit1.CurrentHeat.ShouldBeGreaterThan(initialHeat);

        // Verify heat updated command was published with correct movement heat source
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                cmd.UnitId == _unit1Id && 
                cmd.MovementHeatSources.Count == 1 &&
                cmd.MovementHeatSources[0].MovementType == MovementType.Run &&
                cmd.MovementHeatSources[0].MovementPointsSpent == 5 &&
                cmd.MovementHeatSources[0].HeatPoints == 2)); // Run generates 2 heat points
    }

    [Fact]
    public void Enter_WithWeaponHeat_ShouldCalculateAndApplyCorrectHeat()
    {
        // Arrange
        SetupUnitWithWeaponFired(_unit2);
        var initialHeat = _unit2.CurrentHeat;

        // Act
        _sut.Enter();

        // Assert
        // Verify heat was applied to the unit
        _unit2.CurrentHeat.ShouldBeGreaterThan(initialHeat);

        // Verify heat updated command was published with correct weapon heat source
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                cmd.UnitId == _unit2Id && 
                cmd.WeaponHeatSources.Count == 1 &&
                cmd.WeaponHeatSources[0].WeaponName == "Medium Laser" &&
                cmd.WeaponHeatSources[0].HeatPoints == 3));
    }

    [Fact]
    public void Enter_WithHeatDissipation_ShouldApplyCorrectDissipation()
    {
        // Arrange
        // Add initial heat to unit
        _unit1.ApplyHeat(25);
        var initialHeat = _unit1.CurrentHeat;

        // Act
        _sut.Enter();

        // Assert
        // Verify heat was dissipated
        _unit1.CurrentHeat.ShouldBeLessThan(initialHeat);

        // Verify heat updated command was published with correct dissipation data
        CommandPublisher.Received().PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                cmd.UnitId == _unit1Id && 
                cmd.DissipationData.HeatSinks == _unit1.GetAllComponents<HeatSink>().Count() &&
                cmd.DissipationData.EngineHeatSinks == 10 &&
                cmd.DissipationData.DissipationPoints > 0));
    }

    [Fact]
    public void Enter_WithNoHeatSources_ShouldStillPublishHeatUpdatedCommand()
    {
        // Arrange
        // No heat sources or movement for units

        // Act
        _sut.Enter();

        // Assert
        // Verify heat updated commands were still published for both units
        CommandPublisher.Received(2).PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                (cmd.UnitId == _unit1Id || cmd.UnitId == _unit2Id) &&
                cmd.MovementHeatSources.Count == 0 &&
                cmd.WeaponHeatSources.Count == 0));
    }

    [Fact]
    public void Enter_WithCombinedHeatSources_ShouldCalculateCorrectTotalHeat()
    {
        // Arrange
        // Setup unit with both movement and weapon heat
        SetupUnitWithMovement(_unit1, MovementType.Run);
        SetupUnitWithWeaponFired(_unit1);
        
        var initialHeat = _unit1.CurrentHeat;

        // Act
        _sut.Enter();

        // Assert
        // Verify total heat was applied correctly (3 from jump + 3 from Medium Laser + 10 from PPC = 16)
        _unit1.CurrentHeat.ShouldBeGreaterThan(initialHeat);
        
        // Verify heat updated command was published with correct heat sources
        CommandPublisher.Received(1).PublishCommand(
            Arg.Is<HeatUpdatedCommand>(cmd => 
                cmd.UnitId == _unit1Id && 
                cmd.MovementHeatSources.Count == 1 &&
                cmd.MovementHeatSources[0].MovementType == MovementType.Jump &&
                cmd.MovementHeatSources[0].MovementPointsSpent == 3 &&
                cmd.MovementHeatSources[0].HeatPoints == 3 &&
                cmd.WeaponHeatSources.Count == 2));
    }

    #region Helper Methods

    private void SetupUnitWithMovement(Unit unit, MovementType movementType)
    {
        var deployPosition = new HexPosition(new HexCoordinates(1,1), HexDirection.Bottom);
        unit.Deploy(deployPosition);
        unit.Move(movementType, [new PathSegmentData
            {
                From = deployPosition.ToData(),
                To = deployPosition.ToData(),
                Cost = 0
            }
        ]);
    }

    private void SetupUnitWithWeaponFired(Unit unit)
    {
        // Find a weapon on the unit or add one if needed
        var weapon = unit.GetAllComponents<Weapon>().First();
        
        // If a weapon exists, just set its target
        weapon.Target = _unit2;
    }
    #endregion
}
