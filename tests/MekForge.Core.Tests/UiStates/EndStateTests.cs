using Shouldly;
using NSubstitute;
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
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class EndStateTests
{
    private readonly EndState _sut;
    private readonly ClientGame _game;
    private readonly Unit _unit1;
    private readonly Player _player;
    private readonly BattleMapViewModel _viewModel;
    private readonly ICommandPublisher _commandPublisher;

    public EndStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        
        // Mock localization service responses
        localizationService.GetString("EndPhase_ActionLabel").Returns("End your turn");
        
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        var playerId = Guid.NewGuid();
        
        var rules = new ClassicBattletechRulesProvider();
        var unitData = MechFactoryTests.CreateDummyMechData();
        
        _player = new Player(playerId, "Player1");
        _commandPublisher = Substitute.For<ICommandPublisher>();
        
        _game = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [_player], 
            rules,
            _commandPublisher, 
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = _game;
        
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [unitData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id
        });
        _unit1 = _viewModel.Units.First();
    
        SetPhase(PhaseNames.End);
        _sut = new EndState(_viewModel);
    }

    [Fact]
    public void InitialState_HasEndTurnAction()
    {
        // Assert
        _sut.ActionLabel.ShouldBe("End your turn");
    }

    [Fact]
    public void IsActionRequired_ReturnsFalse_WhenNotActivePlayer()
    {
        // Assert
        _sut.IsActionRequired.ShouldBeFalse();
    }

    [Fact]
    public void HandleHexSelection_SelectsUnitAtHex()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        var hex = new Hex(position.Coordinates);

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(_unit1);
    }

    [Fact]
    public void HandleHexSelection_DeselectsUnit_WhenNoUnitAtHex()
    {
        // Arrange
        _viewModel.SelectedUnit = _unit1;
        var hex = new Hex(new HexCoordinates(2, 2));

        // Act
        _sut.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBeNull();
    }

    [Fact]
    public void ExecutePlayerAction_SendsTurnEndedCommand_WhenActivePlayer()
    {
        // Arrange
        SetActivePlayer();
        
        // Act
        _sut.ExecutePlayerAction();

        // Assert
        _commandPublisher.Received(1).PublishCommand(Arg.Is<TurnEndedCommand>(cmd =>
            cmd.PlayerId == _player.Id &&
            cmd.GameOriginId == _game.Id));
    }

    [Fact]
    public void ExecutePlayerAction_DoesNotSendCommand_WhenNotActivePlayer()
    {
        // Arrange
        var otherPlayerId = Guid.NewGuid();
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = otherPlayerId,
            UnitsToPlay = 0
        });

        // Act
        _sut.ExecutePlayerAction();

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<TurnEndedCommand>());
    }

    [Fact]
    public void ExecutePlayerAction_DoesNotSendCommand_WhenGameIsNull()
    {
        // Arrange
        _viewModel.Game = null;

        // Act
        _sut.ExecutePlayerAction();

        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<TurnEndedCommand>());
    }
    
    private void SetActivePlayer()
    {
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id,
            UnitsToPlay = 0
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
}
