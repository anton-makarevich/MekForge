using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
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

    public DeploymentStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        var viewModel = Substitute.For<BattleMapViewModel>(imageService, localizationService);
        var builder = new DeploymentCommandBuilder(Guid.NewGuid(), Guid.NewGuid());
        _state = new DeploymentState(viewModel, builder);
        
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
        
        viewModel.Game = _game;
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Arrange
        SetActivePlayer();

        // Assert
        _state.ActionLabel.Should().Be("Select Unit");
        _state.IsActionRequired.Should().BeTrue();
    }

    private void SetActivePlayer()
    {
        var player = _game.LocalPlayers[0];
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
        // Arrange
        SetActivePlayer();
        
        // Act
        _state.HandleUnitSelection(_unit);

        // Assert
        _state.ActionLabel.Should().Be("Select Hex");
    }

    [Fact]
    public void HandleHexSelection_ForDeployment_SetsPositionAndHighlightsAdjacent()
    {
        // Arrange
        SetActivePlayer();
        _state.HandleUnitSelection(_unit); // Move to hex selection state

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
        SetActivePlayer();
        var nonAdjacentHex = new Hex(new HexCoordinates(5, 5));
        _state.HandleUnitSelection(_unit);
        _state.HandleHexSelection(_hex1);

        // Act
        _state.HandleHexSelection(nonAdjacentHex);

        // Assert
        _state.ActionLabel.Should().Be("Select Direction");
    }
}
