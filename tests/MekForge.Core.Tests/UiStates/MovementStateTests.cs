using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class MovementStateTests
{
    private readonly MovementState _state;
    private readonly BattleMapViewModel _viewModel;
    private readonly ClientGame _game;
    private readonly Unit _unit;
    private readonly Hex _hex1;
    private readonly Hex _hex2;

    public MovementStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        _viewModel = Substitute.For<BattleMapViewModel>(imageService, localizationService);
        var playerId = Guid.NewGuid();
        var builder = new MoveUnitCommandBuilder(Guid.NewGuid(),  playerId);
        _state = new MovementState(_viewModel, builder);
        
        var rules = new ClassicBattletechRulesProvider();
        _unit = new MechFactory(rules).Create(MechFactoryTests.CreateDummyMechData());
        
        // Create two adjacent hexes
        _hex1 = new Hex(new HexCoordinates(1, 1));
        _hex2 = new Hex(new HexCoordinates(1, 2)); 
        
        var battleMap = new BattleMap(1, 1);
        var player = new Player(playerId, "Player1");
        _game = new ClientGame(
            battleMap, [player], rules,
            Substitute.For<ICommandPublisher>());
        
        _viewModel.Game = _game;
    }

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Arrange
        SetActivePlayer();

        // Assert
        _state.ActionLabel.Should().Be("Select unit to move");
        _state.IsActionRequired.Should().BeTrue();
    }

    private void SetActivePlayer()
    {
        var player = _game.LocalPlayers[0];
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });
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
    public void HandleUnitSelection_TransitionsToMovementTypeSelection()
    {
        // Arrange
        SetActivePlayer();
        
        // Act
        _state.HandleUnitSelection(_unit);

        // Assert
        _state.ActionLabel.Should().Be("Select movement type");
        _viewModel.Received(1).NotifyStateChanged();
    }

    [Fact]
    public void HandleMovementTypeSelection_TransitionsToHexSelection()
    {
        // Arrange
        SetActivePlayer();
        _state.HandleUnitSelection(_unit);
        
        // Act
        _state.HandleMovementTypeSelection(MovementType.Walk);

        // Assert
        _state.ActionLabel.Should().Be("Select target hex");
        _viewModel.Received(2).NotifyStateChanged();
    }

    [Fact]
    public void HandleHexSelection_TransitionsToDirectionSelection()
    {
        // Arrange
        SetActivePlayer();
        _state.HandleUnitSelection(_unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        
        // Act
        _state.HandleHexSelection(_hex1);

        // Assert
        _state.ActionLabel.Should().Be("Select facing direction");
        _viewModel.Received(1).HighlightHexes(Arg.Any<List<HexCoordinates>>(), true);
    }

    [Fact]
    public void HandleHexSelection_CompletesMovement_WhenDirectionSelected()
    {
        // Arrange
        SetActivePlayer();
        _state.HandleUnitSelection(_unit);
        _state.HandleMovementTypeSelection(MovementType.Walk);
        _state.HandleHexSelection(_hex1);
        
        // Act
        _state.HandleHexSelection(_hex2);

        // Assert
        _state.ActionLabel.Should().Be(string.Empty);
        _state.IsActionRequired.Should().BeFalse();
        _viewModel.Received(1).HighlightHexes(Arg.Any<List<HexCoordinates>>(), false);
    }
}
