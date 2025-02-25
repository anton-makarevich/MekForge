using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class MovementStateTests
{
    private readonly MovementState _state;
    private readonly ClientGame _game;
    private readonly UnitData _unitData;
    private readonly Unit _unit1;
    private readonly Unit _unit2;
    private readonly Player _player;
    private readonly Hex _hex1;
    private readonly BattleMapViewModel _viewModel;

    public MovementStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        var playerId = Guid.NewGuid();
        
        
        var rules = new ClassicBattletechRulesProvider();
        _unitData = MechFactoryTests.CreateDummyMechData();
        var ct = _unitData.LocationEquipment[PartLocation.CenterTorso];
        ct.AddRange(MekForgeComponent.JumpJet,MekForgeComponent.JumpJet);
        _unit1 = new MechFactory(rules).Create(_unitData);
        _unit2 = new MechFactory(rules).Create(_unitData);
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2,11, new ClearTerrain()));
         _player = new Player(playerId, "Player1");
        _game = new ClientGame(
            battleMap, [_player], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = _game;
        AddPlayerUnits();
        SetActivePlayer();
        _state = new MovementState(_viewModel);
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.ShouldBe("Select unit to move");
        _state.IsActionRequired.ShouldBeTrue();
    }

    private void AddPlayerUnits()
    {
        var playerId2 = Guid.NewGuid();
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [_unitData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id
        });
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [_unitData],
            Tint = "#FFFF00",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId2
        });
        _game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id
        });
        _game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId2
        });
    }
    private void SetActivePlayer()
    {
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id,
            UnitsToPlay = 1
        });
    }
    
    private void SetPhase(PhaseNames phase)
    {
        _game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = phase,
        });
    }

    [Fact]
    public void HandleUnitSelection_TransitionsToMovementTypeSelection()
    {
        // Act
        _state.HandleUnitSelection(_unit1);

        // Assert
        _state.ActionLabel.ShouldBe("Select movement type");
    }

    [Fact]
    public void HandleMovementTypeSelection_TransitionsToHexSelection()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Walk);

        // Assert
        _state.ActionLabel.ShouldBe("Select target hex");
    }

    [Fact]
    public void HandleHexSelection_TransitionsToDirectionSelection()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(new HexCoordinates(1,2),HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.ShouldBe("Select facing direction");
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var state = _viewModel.CurrentState;
        var position = new HexPosition(new HexCoordinates(1, 1),HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);

        // Act
        state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
        state.ActionLabel.ShouldBe("Select movement type");
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex_AndOtherUnitIsSelected()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var state = _viewModel.CurrentState;
        var position = new HexPosition(new HexCoordinates(1, 1),HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);
        _viewModel.SelectedUnit = _unit2;

        // Act
        state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
        state.ActionLabel.ShouldBe("Select movement type");
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectsUnit_WhenUnitIsOnHex_ButNotOwnedByActivePlayer()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var state = _viewModel.CurrentState;
        var position = new HexCoordinates(1, 1);
        var unit = _viewModel.Units.Last();
        unit.Deploy(new HexPosition(position,HexDirection.Bottom));
        var hex = new Hex(position);

        // Act
        state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_DoesNothing_WhenNoUnitOnHex()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(1, 1));
        _unit1.Deploy(new HexPosition(1, 2, HexDirection.Bottom));
        var newPosition = new HexPosition(new HexCoordinates(2, 2), HexDirection.Top);
        _unit1.Move(MovementType.Walk,
            [new PathSegment(new HexPosition(1, 2, HexDirection.Bottom), newPosition, 1)
                .ToData()]);

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _state.ActionLabel.ShouldBe("Select unit to move");
    }

    [Fact]
    public void Constructor_ShouldThrow_IfGameNull()
    {
        // Arrange
        _viewModel.Game=null;
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new MovementState(_viewModel));
    }

    [Fact]
    public void Constructor_ShouldThrow_IfActivePlayerNull()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new MovementState(_viewModel));
    }

    [Fact]
    public void HandleTargetHexSelection_ShowsDirectionSelector_WithPossibleDirections()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        _state.HandleUnitSelection(unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 2))!;
        
        // Act
        _state.HandleHexSelection(targetHex);
        
        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.DirectionSelectorPosition.ShouldBe(targetHex.Coordinates);
        _viewModel.AvailableDirections.ShouldNotBeEmpty();
        // All directions should be available for adjacent hex with clear terrain
        _viewModel.AvailableDirections.ToList().Count.ShouldBe(6);
    }

    [Fact]
    public void HandleHexSelection_ResetsHighlighting_WhenUnitIsReselected()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        _viewModel.SelectedUnit = unit;
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 2))!;
        _state.HandleHexSelection(targetHex);
        
        // Act
        _state.HandleHexSelection(new Hex(position.Coordinates));
        
        // Assert
        foreach (var hex in _viewModel.Game!.BattleMap.GetHexes())
        {
            hex.IsHighlighted.ShouldBeFalse();
        }
    }

    [Fact]
    public void HandleTargetHexSelection_DoesNotShowDirectionSelector_ForUnreachableHex()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        _state.HandleUnitSelection(unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        // Create a hex that is too far to reach
        var unreachableHex = new Hex(new HexCoordinates(10, 10));
        
        // Act
        _state.HandleHexSelection(unreachableHex);
        
        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
        _viewModel.AvailableDirections.ShouldBeNull();
    }

    [Fact]
    public void HandleFacingSelection_DisplaysPath_WhenInDirectionSelectionStep()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        _state.HandleUnitSelection(unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 2))!;
        _state.HandleHexSelection(targetHex);
        
        // Act
        _state.HandleFacingSelection(HexDirection.Top);
        
        // Assert
        _viewModel.MovementPath.ShouldNotBeNull();
        _viewModel.MovementPath.Last().To.Coordinates.ShouldBe(targetHex.Coordinates);
        _viewModel.MovementPath.Last().To.Facing.ShouldBe(HexDirection.Top);
    }

    [Fact]
    public void HandleFacingSelection_CompletesMovement_WhenSelectedSecondTime()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        _state.HandleUnitSelection(unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 2))!;
        _state.HandleHexSelection(targetHex);
        
        // Act
        _state.HandleFacingSelection(HexDirection.Top);
        _state.HandleFacingSelection(HexDirection.Top);
        
        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
        _state.ActionLabel.ShouldBeEmpty();
        _state.IsActionRequired.ShouldBeFalse();
        foreach (var hex in _viewModel.Game!.BattleMap.GetHexes())
        {
            hex.IsHighlighted.ShouldBeFalse();
        }
    }

    [Fact]
    public void HandleFacingSelection_DoesNothing_WhenNotInDirectionSelectionStep()
    {
        // Arrange
        SetPhase(PhaseNames.Movement);
        SetActivePlayer();
        
        // Act
        _state.HandleFacingSelection(HexDirection.Top);
        
        // Assert
        _state.ActionLabel.ShouldBe("Select unit to move");
    }

    [Fact]
    public void HandleMovementTypeSelection_CalculatesBackwardReachableHexes_WhenWalking()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Walk);

        // Assert
        // The hex behind the unit (at 1,2) should be reachable
        var hexBehind = _game.BattleMap.GetHex(new HexCoordinates(1, 9));
        hexBehind!.IsHighlighted.ShouldBeTrue();
    }

    [Fact]
    public void HandleMovementTypeSelection_DoesNotCalculateBackwardReachableHexes_WhenRunning()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Run);

        // Assert
        // The hex behind the unit (at 1,11) should not be reachable (12 running MP are not enough to reach it)
        var hexBehind = _game.BattleMap.GetHex(new HexCoordinates(1, 11));
        hexBehind!.IsHighlighted.ShouldBeFalse();
    }

    [Fact]
    public void HandleTargetHexSelection_AllowsBackwardMovement_WhenWalking()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 2))!;

        // Act
        _state.HandleHexSelection(targetHex);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.DirectionSelectorPosition.ShouldBe(targetHex.Coordinates);
    }

    [Fact]
    public void HandleTargetHexSelection_SwapsDirectionsForBackwardMovement()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 4))!;
        _state.HandleHexSelection(targetHex);

        // Act
        _state.HandleFacingSelection(HexDirection.Top);

        // Assert
        // Check that the path segments have correct facing directions
        var path = _viewModel.MovementPath;
        path.ShouldNotBeNull();
        path[0].From.Facing.ShouldBe(HexDirection.Top); // Original facing
        path[0].To.Coordinates.ShouldBe(new HexCoordinates(1, 2)); // Target hex
        path[0].To.Facing.ShouldBe(HexDirection.Top); // Maintains facing for backward movement
        path.Last().To.Coordinates.ShouldBe(targetHex.Coordinates);
        path.Last().To.Facing.ShouldBe(HexDirection.Top);
    }
    
    [Fact]
    public void HandleUnitSelection_ClearsHexHighlighting_WhenUnitSelectedAgain()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var unit = _viewModel.Units.First();
        unit.Deploy(startPosition);
        var unitHex = _game.BattleMap.GetHex(unit.Position!.Value.Coordinates)!;
        _state.HandleHexSelection(unitHex);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 4))!;
        _state.HandleHexSelection(targetHex);
        _state.HandleFacingSelection(HexDirection.Top);

        // Act
        _state.HandleHexSelection(unitHex);
        
        // Assert
        _viewModel.MovementPath.ShouldBeNull();
        foreach (var hex in _viewModel.Game!.BattleMap.GetHexes())
        {
            hex.IsHighlighted.ShouldBeFalse();
        }
    }

    [Fact]
    public void HandleMovementTypeSelection_ForJumping_CalculatesReachableHexes()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Jump);

        // Assert
        var reachableHexes = _game.BattleMap.GetHexes()
            .Count(h => h.IsHighlighted);
        reachableHexes.ShouldBeGreaterThan(0, "Should highlight reachable hexes");

        // Verify only hexes within jump range are highlighted
        foreach (var hex in _game.BattleMap.GetHexes())
        {
            if (hex.IsHighlighted)
            {
                hex.Coordinates.DistanceTo(position.Coordinates)
                    .ShouldBeLessThanOrEqualTo(_unit1.GetMovementPoints(MovementType.Jump),
                        "Should only highlight hexes within jump range");
            }
        }
    }

    [Fact]
    public void HandleTargetHexSelection_ForJumping_ShowsAllDirections()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Jump);
        
        var targetHex = _game.BattleMap.GetHex(new HexCoordinates(1, 3))!;
        
        // Act
        _state.HandleHexSelection(targetHex);
        
        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue("Should show direction selector");
        _viewModel.DirectionSelectorPosition.ShouldBe(targetHex.Coordinates);
        _viewModel.AvailableDirections!.ToList().Count.ShouldBe(6, "All directions should be available for jumping");
    }

    [Fact]
    public void HandleMovementTypeSelection_CompletesMovement_WhenStandingStill()
    {
        // Arrange
        var startPosition = new HexPosition(new HexCoordinates(1,2), HexDirection.Bottom);
        _unit1.Deploy(startPosition);
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.StandingStill);

        // Assert
        _state.ActionLabel.ShouldBe(string.Empty); // Movement should be completed
    }

    [Fact]
    public void HandleMovementTypeSelection_IncludesCurrentHex_InReachableHexes()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Walk);

        // Assert
        var reachableHexes = _viewModel.Game.BattleMap.GetHexes()
            .Where(h => h.IsHighlighted)
            .Select(h => h.Coordinates)
            .ToList();
        reachableHexes.ShouldContain(position.Coordinates);
    }

    [Fact]
    public void HandleTargetHexSelection_ShowsDirectionSelector_WhenSelectingCurrentHex()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var currentHex = _viewModel.Game.BattleMap.GetHex(position.Coordinates);

        // Act
        _state.HandleHexSelection(currentHex);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.DirectionSelectorPosition.ShouldBe(position.Coordinates);
    }

    [Fact]
    public void HandleTargetHexSelection_ResetsSelection_WhenClickingOutsideReachableHexes()
    {
        // Arrange
        var unit = _viewModel.Units.First();
        var startPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Top);
        unit.Deploy(startPosition);
        var unitHex = _game.BattleMap.GetHex(unit.Position!.Value.Coordinates)!;
        _state.HandleHexSelection(unitHex);
        _state.HandleUnitSelection(unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        var unreachableHex = _viewModel.Game!.BattleMap.GetHex(new HexCoordinates(1, 11)); // Far away hex

        // Act
        _state.HandleHexSelection(unreachableHex!);

        // Assert
        _viewModel.SelectedUnit.ShouldBeNull(); // Selection should be reset
        _state.CurrentMovementStep.ShouldBe(MovementStep.SelectingUnit); // Back to initial step
        _viewModel.Game.BattleMap.GetHexes()
            .Any(h => h.IsHighlighted)
            .ShouldBeFalse(); // No highlighted hexes
    }

    [Fact]
    public void GetAvailableActions_NoSelectedUnit_ReturnsEmpty()
    {
        // Act
        var actions = _state.GetAvailableActions();

        // Assert
        actions.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailableActions_NotInMovementTypeSelection_ReturnsEmpty()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);
        _state.HandleMovementTypeSelection(MovementType.Walk); // This moves us past movement type selection

        // Act
        var actions = _state.GetAvailableActions();

        // Assert
        actions.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailableActions_InMovementTypeSelection_ReturnsMovementOptions()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);

        // Act
        var actions = _state.GetAvailableActions().ToList();

        // Assert
        actions.Count.ShouldBe(4); // Stand Still, Walk, Run, Jump (since unit has jump jets)
        actions[0].Label.ShouldBe("Stand Still");
        actions[1].Label.ShouldBe($"Walk | MP: {_unit1.GetMovementPoints(MovementType.Walk)}");
        actions[2].Label.ShouldBe($"Run | MP: {_unit1.GetMovementPoints(MovementType.Run)}");
        actions[3].Label.ShouldBe($"Jump | MP: {_unit1.GetMovementPoints(MovementType.Jump)}");
    }

    [Fact]
    public void GetAvailableActions_NoJumpJets_DoesNotShowJumpOption()
    {
        // Arrange
        var unitData = MechFactoryTests.CreateDummyMechData();
        var rules = new ClassicBattletechRulesProvider();
        var unitWithoutJumpJets = new MechFactory(rules).Create(unitData);
        _state.HandleUnitSelection(unitWithoutJumpJets);

        // Act
        var actions = _state.GetAvailableActions().ToList();

        // Assert
        actions.Count.ShouldBe(3); // Stand Still, Walk, Run
        actions.ShouldNotContain(a => a.Label.StartsWith("Jump"));
    }

    [Fact]
    public void GetAvailableActions_ExecutingAction_UpdatesState()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var walkAction = actions.First(a => a.Label.StartsWith("Walk"));

        // Act
        walkAction.OnExecute();

        // Assert
        _state.CurrentMovementStep.ShouldBe(MovementStep.SelectingTargetHex);
    }
}
