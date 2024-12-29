using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class DeploymentStateTests
{
    private readonly DeploymentState _state;
    private readonly BattleMapViewModel _viewModel;
    private readonly Unit _unit;
    private readonly Hex _hex1;
    private readonly Hex _hex2;

    public DeploymentStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        _viewModel = Substitute.For<BattleMapViewModel>(imageService);
        var builder = new DeploymentCommandBuilder(Guid.NewGuid(), Guid.NewGuid());
        _state = new DeploymentState(_viewModel, builder);
        
        _unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(MechFactoryTests.CreateDummyMechData());
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        _hex2 = new Hex(new HexCoordinates(1, 2)); 
        
        var game = Substitute.For<IGame>();
        _viewModel.Game = game;
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.Should().Be("Select Unit");
        _state.IsActionRequired.Should().BeTrue();
    }

    [Fact]
    public void HandleUnitSelection_DoesNothing_WhenUnitIsNull()
    {
        // Act
        _state.HandleUnitSelection(null);

        // Assert
        _viewModel.DidNotReceive().NotifyStateChanged();
    }

    [Fact]
    public void HandleUnitSelection_TransitionsToHexSelection()
    {
        // Act
        _state.HandleUnitSelection(_unit);

        // Assert
        _state.ActionLabel.Should().Be("Select Hex");
        _viewModel.Received(1).NotifyStateChanged();
    }

    [Fact]
    public void HandleHexSelection_ForDeployment_SetsPositionAndHighlightsAdjacent()
    {
        // Arrange
        _state.HandleUnitSelection(_unit); // Move to hex selection state

        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.Should().Be("Select Direction");
        _viewModel.Received(2).NotifyStateChanged(); // Once for unit selection, once for hex selection
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
}
