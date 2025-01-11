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
        _state.ActionLabel.Should().Be("Select Unit");
        _state.IsActionRequired.Should().BeTrue();
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
        _state.ActionLabel.Should().Be("Select Hex");
    }

    [Fact]
    public void HandleHexSelection_ForDeployment_SetsPositionAndHighlightsAdjacent()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.Should().Be("Select Direction");
    }

    [Fact]
    public void HandleHexSelection_ForDirection_CompletesDeployment_WhenHexIsAdjacent()
    {
        // Arrange
        _state.HandleUnitSelection(_unit);
        _state.HandleHexSelection(_hex1);

        // Act
        _state.HandleHexSelection(_hex2);

        // Assert
        _state.ActionLabel.Should().Be("");
    }

    [Fact]
    public void HandleHexSelection_ForDirection_DoesNothing_WhenHexIsNotAdjacent()
    {
        // Arrange
        var nonAdjacentHex = new Hex(new HexCoordinates(5, 5));
        _state.HandleUnitSelection(_unit);
        _state.HandleHexSelection(_hex1);

        // Act
        _state.HandleHexSelection(nonAdjacentHex);

        // Assert
        _state.ActionLabel.Should().Be("Select Direction");
    }
    
    [Fact]
    public void Constructor_ShouldThrow_IfGameNull()
    {
        // Arrange
        _viewModel.Game=null;
        // Act
        var action = () => new DeploymentState(_viewModel);
        // Assert
        action.Should().Throw<InvalidOperationException>();
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
        // Act
        var action = () => new DeploymentState(_viewModel);
        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
