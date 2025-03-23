using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.Utils;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class DeploymentStateTests
{
    private DeploymentState _sut;
    private readonly ClientGame _game;
    private readonly Unit _unit;
    private readonly Hex _hex1;
    private readonly Hex _hex2;
    private readonly BattleMapViewModel _viewModel;

    public DeploymentStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        
        _viewModel = new BattleMapViewModel(imageService, localizationService);

        var rules = new ClassicBattletechRulesProvider();
        var unitData = MechFactoryTests.CreateDummyMechData();
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        _hex2 = new Hex(new HexCoordinates(1, 2)); 
        
        var battleMap = new BattleMap(1, 1);
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(
            battleMap, [player], rules,
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = _game;
        SetActivePlayer(player, unitData);
        _unit = _viewModel.Units.First();
        _sut = new DeploymentState(_viewModel);

        localizationService.GetString("Action_SelectUnitToDeploy").Returns("Select Unit");
        localizationService.GetString("Action_SelectDeploymentHex").Returns("Select Hex");
        localizationService.GetString("Action_SelectFacingDirection").Returns("Select facing direction");
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _sut.ActionLabel.ShouldBe("Select Unit");
        _sut.IsActionRequired.ShouldBeTrue();
    }

    private void SetActivePlayer(Player player, UnitData unitData)
    {
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = player.Name,
            Units = [unitData],
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            Tint = "#FF0000"
        });
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });
    }

    [Fact]
    public void HandleUnitSelection_TransitionsToHexSelection()
    {
        // Act
        _sut.HandleUnitSelection(_unit);

        // Assert
        _sut.ActionLabel.ShouldBe("Select Hex");
    }

    [Fact]
    public void HandleHexSelection_ForDeployment_UpdatesStepToSelectDirection()
    {
        // Arrange
        _sut.HandleUnitSelection(_unit);
        
        // Act
        _sut.HandleHexSelection(_hex1);

        // Assert
        _sut.ActionLabel.ShouldBe("Select facing direction");
    }
    
    [Fact]
    public void Constructor_ShouldThrow_IfGameNull()
    {
        // Arrange
        _viewModel.Game=null;
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new DeploymentState(_viewModel));
    }
    
    [Fact]
    public void Constructor_ShouldThrow_IfActivePlayerNull()
    {
        // Arrange
        _game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.WeaponsAttack,
        });
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new DeploymentState(_viewModel));
    }

    [Fact]
    public void HandleHexSelection_WhenSelectingHex_ShowsDirectionSelector()
    {
        // Arrange
        _sut.HandleUnitSelection(_unit);
        
        // Act
        _sut.HandleHexSelection(_hex1);

        // Assert
        _viewModel.DirectionSelectorPosition.ShouldBe(_hex1.Coordinates);
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.AvailableDirections!.ToList().Count.ShouldBe(6);
    }
    
    [Fact]
    public void HandleHexSelection_WhenSelectingHexTwice_ShouldSelectSecondHex()
    {
        // Arrange
        _sut.HandleUnitSelection(_unit);
        
        // Act
        _sut.HandleHexSelection(_hex1);
        _sut.HandleHexSelection(_hex2);

        // Assert
        _viewModel.DirectionSelectorPosition.ShouldBe(_hex2.Coordinates);
    }
    
    [Fact]
    public void HandleFacingSelection_WhenDirectionSelected_CompletesDeployment()
    {
        // Arrange
        _sut.HandleUnitSelection(_unit);
        _sut.HandleHexSelection(_hex1);
    
        // Act
        _sut.HandleFacingSelection(HexDirection.Top);
    
        // Assert
        _sut.ActionLabel.ShouldBe("");
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }

    [Fact]
    public void HandleFacingSelection_AfterSelection_HidesDirectionSelector()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(1, 1));
        _sut.HandleHexSelection(hex);

        // Act
        _sut.HandleFacingSelection(HexDirection.Top);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_WhenHexIsOccupied_ShouldNotShowDirectionSelector()
    {
        // Arrange
        _sut.HandleUnitSelection(_unit);
        // Deploy first unit
        _sut.HandleHexSelection(_hex1);
        _sut.HandleFacingSelection(HexDirection.Top);
        _unit.Deploy(new HexPosition(_hex1.Coordinates,HexDirection.Top));
        
        // Try to deploy second unit to the same hex
        var secondUnit = new MechFactory(new ClassicBattletechRulesProvider()).Create(MechFactoryTests.CreateDummyMechData());
        _sut = new DeploymentState(_viewModel);
        _sut.HandleUnitSelection(secondUnit);
        
        // Act
        _sut.HandleHexSelection(_hex1);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }
}
