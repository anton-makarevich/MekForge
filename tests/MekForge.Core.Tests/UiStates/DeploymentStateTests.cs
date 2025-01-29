using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
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
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class DeploymentStateTests
{
    private readonly DeploymentState _state;
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
        _unit = new MechFactory(rules).Create(MechFactoryTests.CreateDummyMechData());
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        _hex2 = new Hex(new HexCoordinates(1, 2)); 
        
        var battleMap = new BattleMap(1, 1);
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(
            battleMap, [player], rules,
            Substitute.For<ICommandPublisher>());
        
        _viewModel.Game = _game;
        SetActivePlayer(player);
        _state = new DeploymentState(_viewModel);
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.ShouldBe("Select Unit");
        _state.IsActionRequired.ShouldBeTrue();
    }

    private void SetActivePlayer(Player player)
    {
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = player.Name,
            Units = [],
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
        _state.HandleUnitSelection(_unit);

        // Assert
        _state.ActionLabel.ShouldBe("Select Hex");
    }

    [Fact]
    public void HandleHexSelection_ForDeployment_UpdatesStepToSelectDirection()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.ShouldBe("Select Direction");
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
            Phase = PhaseNames.Attack,
        });
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new DeploymentState(_viewModel));
    }

    [Fact]
    public void HandleHexSelection_WhenSelectingHex_ShowsDirectionSelector()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _viewModel.DirectionSelectorPosition.ShouldBe(_hex1.Coordinates);
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.AvailableDirections!.ToList().Count.ShouldBe(6);
    }
    
    [Fact]
    public void HandleHexSelection_WhenSelectingHexTwice_ShouldSelectSecondHex()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleHexSelection(_hex1);
        _state.HandleHexSelection(_hex2);

        // Assert
        _viewModel.DirectionSelectorPosition.ShouldBe(_hex2.Coordinates);
    }
    
    [Fact]
    public void HandleFacingSelection_WhenDirectionSelected_CompletesDeployment()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        _state.HandleHexSelection(_hex1);
    
        // Act
        _state.HandleFacingSelection(HexDirection.Top);
    
        // Assert
        _state.ActionLabel.ShouldBe("");
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }

    [Fact]
    public void HandleFacingSelection_AfterSelection_HidesDirectionSelector()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(1, 1));
        _state.HandleHexSelection(hex);

        // Act
        _state.HandleFacingSelection(HexDirection.Top);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_WhenHexIsOccupied_ShouldNotShowDirectionSelector()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        // Deploy first unit
        _state.HandleHexSelection(_hex1);
        _state.HandleFacingSelection(HexDirection.Top);
        
        // Try to deploy second unit to the same hex
        var secondUnit = new MechFactory(new ClassicBattletechRulesProvider()).Create(MechFactoryTests.CreateDummyMechData());
        _state.HandleUnitSelection(secondUnit);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
    }
}
