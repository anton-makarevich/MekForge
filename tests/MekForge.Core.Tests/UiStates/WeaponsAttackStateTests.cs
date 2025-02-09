using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
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

public class WeaponsAttackStateTests
{
    private readonly WeaponsAttackState _state;
    private readonly ClientGame _game;
    private readonly UnitData _unitData;
    private readonly Unit _unit1;
    private readonly Unit _unit2;
    private readonly Player _player;
    private readonly BattleMapViewModel _viewModel;

    public WeaponsAttackStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        var playerId = Guid.NewGuid();
        
        var rules = new ClassicBattletechRulesProvider();
        _unitData = MechFactoryTests.CreateDummyMechData();
        _unit1 = new MechFactory(rules).Create(_unitData);
        _unit2 = new MechFactory(rules).Create(_unitData);
        
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2,11, new ClearTerrain()));
        _player = new Player(playerId, "Player1");
        _game = new ClientGame(
            battleMap, [_player], rules,
            Substitute.For<ICommandPublisher>());
        
        _viewModel.Game = _game;
        AddPlayerUnits();
        SetActivePlayer();
        _state = new WeaponsAttackState(_viewModel);
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
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.ShouldBe("Select unit to fire weapons");
        _state.IsActionRequired.ShouldBeTrue();
    }

    [Fact]
    public void HandleUnitSelection_TransitionsToActionSelection()
    {
        // Act
        _state.HandleUnitSelection(_unit1);

        // Assert
        _state.ActionLabel.ShouldBe("Select action");
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.ActionSelection);
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex_AndOtherUnitIsSelected()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);
        _viewModel.SelectedUnit = _unit2;

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectUnit_WhenUnitHasFiredWeapons()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var state = _viewModel.CurrentState;
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        unit.FireWeapons();
        var hex = new Hex(position.Coordinates);

        // Act
        state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBeNull();
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
    public void GetAvailableActions_NotInActionSelectionStep_ReturnsEmpty()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1,1,HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        IEnumerable<StateAction> actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");
        torsoAction.OnExecute(); // This puts us in WeaponsConfiguration step

        // Act
        actions = _state.GetAvailableActions();

        // Assert
        actions.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailableActions_InActionSelection_ReturnsTorsoAndTargetOptions()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);

        // Act
        var actions = _state.GetAvailableActions().ToList();

        // Assert
        actions.Count.ShouldBe(2);
        actions[0].Label.ShouldBe("Turn Torso");
        actions[1].Label.ShouldBe("Select Target");
    }

    [Fact]
    public void GetAvailableActions_TorsoRotationAction_TransitionsToWeaponsConfiguration()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1,1,HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");

        // Act
        torsoAction.OnExecute();

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.WeaponsConfiguration);
        _state.ActionLabel.ShouldBe("Configure weapons");
    }

    [Fact]
    public void GetAvailableActions_SelectTargetAction_TransitionsToTargetSelection()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var selectTargetAction = actions.First(a => a.Label == "Select Target");

        // Act
        selectTargetAction.OnExecute();

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.TargetSelection);
        _state.ActionLabel.ShouldBe("Select target");
    }

    [Fact]
    public void HandleFacingSelection_HidesDirectionSelector()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1,1,HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");
        torsoAction.OnExecute();

        // Act
        _state.HandleFacingSelection(HexDirection.BottomLeft);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
        _state.ActionLabel.ShouldBe("Select action");
    }
}
