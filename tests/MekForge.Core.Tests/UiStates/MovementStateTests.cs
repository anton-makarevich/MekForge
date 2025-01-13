using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
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
    private readonly Unit _unit;
    private readonly Player _player;
    private readonly Hex _hex1;
    private readonly Hex _hex2;
    private readonly BattleMapViewModel _viewModel;

    public MovementStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        var playerId = Guid.NewGuid();
        
        
        var rules = new ClassicBattletechRulesProvider();
        _unitData = MechFactoryTests.CreateDummyMechData();
        _unit = new MechFactory(rules).Create(_unitData);
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        _hex2 = new Hex(new HexCoordinates(1, 2)); 
        
        var battleMap = BattleMap.GenerateMap(
            2, 2,
            new SingleTerrainGenerator(2,2, new ClearTerrain()));
         _player = new Player(playerId, "Player1");
        _game = new ClientGame(
            battleMap, [_player], rules,
            Substitute.For<ICommandPublisher>());
        
        _viewModel.Game = _game;
        AddPlayerUnits();
        SetActivePlayer();
        _state = new MovementState(_viewModel);
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.Should().Be("Select unit to move");
        _state.IsActionRequired.Should().BeTrue();
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
        _state.HandleUnitSelection(_unit);

        // Assert
        _state.ActionLabel.Should().Be("Select movement type");
    }

    [Fact]
    public void HandleMovementTypeSelection_TransitionsToHexSelection()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Walk);

        // Assert
        _state.ActionLabel.Should().Be("Select target hex");
    }

    [Fact]
    public void HandleHexSelection_TransitionsToDirectionSelection()
    {
        // Arrange
        _unit.Deploy(new HexPosition(new HexCoordinates(1,2),HexDirection.Bottom));
        _state.HandleUnitSelection(_unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.Should().Be("Select facing direction");
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
        _viewModel.SelectedUnit.Should().Be(unit);
        state.ActionLabel.Should().Be("Select movement type");
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
        _viewModel.SelectedUnit.Should().BeNull();
    }

    [Fact]
    public void HandleHexSelection_DoesNothing_WhenNoUnitOnHex()
    {
        // Arrange
         var hex = new Hex(new HexCoordinates(1, 1));
        _unit.MoveTo(new HexPosition(new HexCoordinates(2, 2),0));

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _state.ActionLabel.Should().Be("Select unit to move");
    }

    [Fact]
    public void Constructor_ShouldThrow_IfGameNull()
    {
        // Arrange
        _viewModel.Game=null;
        // Act
        var action = () => new MovementState(_viewModel);
        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void Constructor_ShouldThrow_IfActivePlayerNull()
    {
        // Arrange
        SetPhase(PhaseNames.Attack);
        // Act
        var action = () => new MovementState(_viewModel);
        // Assert
        action.Should().Throw<InvalidOperationException>();
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
        _viewModel.IsDirectionSelectorVisible.Should().BeTrue();
        _viewModel.DirectionSelectorPosition.Should().Be(targetHex.Coordinates);
        _viewModel.AvailableDirections.Should().NotBeEmpty();
        // All directions should be available for adjacent hex with clear terrain
        _viewModel.AvailableDirections.Should().HaveCount(6);
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
        _viewModel.IsDirectionSelectorVisible.Should().BeFalse();
        _viewModel.AvailableDirections.Should().BeNull();
    }

    [Fact]
    public void HandleFacingSelection_CompletesMovement_WhenInDirectionSelectionStep()
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
        _viewModel.IsDirectionSelectorVisible.Should().BeFalse();
        _state.ActionLabel.Should().BeEmpty();
        _state.IsActionRequired.Should().BeFalse();
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
        _state.ActionLabel.Should().Be("Select unit to move");
    }
}
