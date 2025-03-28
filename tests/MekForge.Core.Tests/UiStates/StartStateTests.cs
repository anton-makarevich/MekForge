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
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class StartStateTests
{
    private readonly StartState _sut;
    private readonly ClientGame _game;
    private readonly Player _localPlayer1;
    private readonly BattleMapViewModel _viewModel;
    private readonly ICommandPublisher _commandPublisher;

    public StartStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();

        // Mock localization service responses
        localizationService.GetString("StartPhase_ActionLabel").Returns("Ready to play");
        localizationService.GetString("StartPhase_PlayerActionLabel").Returns("Ready to play");
        
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        
        var rules = new ClassicBattletechRulesProvider();
        
        _localPlayer1 = new Player(Guid.NewGuid(), "LocalPlayer1") { Status = PlayerStatus.Joining };
        var localPlayer2 = new Player(Guid.NewGuid(), "LocalPlayer2") { Status = PlayerStatus.Joining };
        var localPlayers = new List<IPlayer> { _localPlayer1, localPlayer2 };
        
        _commandPublisher = Substitute.For<ICommandPublisher>();
        
        _game = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            localPlayers, 
            rules,
            _commandPublisher, 
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = _game;
        
        // Set the phase to Start
        _game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.Start
        });
        
        _sut = new StartState(_viewModel);
    }

    [Fact]
    public void InitialState_HasCorrectActionLabel()
    {
        // Assert
        _sut.ActionLabel.ShouldBe("Ready to play");
    }

    [Fact]
    public void IsActionRequired_ReturnsTrue_WhenActivePlayerIsLocal()
    {
        // Arrange - localPlayer1 should be active by default
        
        // Assert
        _sut.IsActionRequired.ShouldBeTrue();
    }
    
    [Fact]
    public void InitialState_CanExecutePlayerAction()
    {
        // Assert
        _sut.CanExecutePlayerAction.ShouldBeTrue();
    }

    [Fact]
    public void IsActionRequired_ReturnsFalse_WhenActivePlayerIsNotLocal()
    {
        // Arrange
        var remotePlayer = new Player(Guid.NewGuid(), "RemotePlayer") { Status = PlayerStatus.Joining };
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerId = remotePlayer.Id,
            PlayerName = remotePlayer.Name,
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        });
        
        // Set remote player as active
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = remotePlayer.Id,
            UnitsToPlay = 0
        });
        
        // Assert
        _sut.IsActionRequired.ShouldBeFalse();
    }

    [Fact]
    public void ExecutePlayerAction_ShouldPublishReadyCommand_ForActiveLocalPlayerOnly()
    {
        // Arrange
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerId = _localPlayer1.Id,
            PlayerName = _localPlayer1.Name,
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        });
        
        // Act
        _sut.ExecutePlayerAction();
        
        // Assert
        _commandPublisher.Received().PublishCommand(Arg.Is<UpdatePlayerStatusCommand>(cmd => 
            cmd.PlayerId == _localPlayer1.Id && 
            cmd.PlayerStatus == PlayerStatus.Playing &&
            cmd.GameOriginId == _game.Id
        ));
    }

    [Fact]
    public void ExecutePlayerAction_ShouldNotPublishCommand_WhenActivePlayerIsNotLocal()
    {
        // Arrange
        var remotePlayer = new Player(Guid.NewGuid(), "RemotePlayer") { Status = PlayerStatus.Joining };
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerId = remotePlayer.Id,
            PlayerName = remotePlayer.Name,
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        });
        
        // Set remote player as active
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = remotePlayer.Id,
            UnitsToPlay = 0
        });
        
        // Act
        _sut.ExecutePlayerAction();
        
        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<UpdatePlayerStatusCommand>());
    }

    [Fact]
    public void ExecutePlayerAction_ShouldNotPublishCommand_WhenGameIsNull()
    {
        // Arrange
        _viewModel.Game = null;
        
        // Act
        _sut.ExecutePlayerAction();
        
        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<UpdatePlayerStatusCommand>());
    }

    [Fact]
    public void ExecutePlayerAction_ShouldNotPublishCommand_WhenActivePlayerIsNull()
    {
        // Arrange
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = Guid.Empty, // Invalid player ID to set active player to null
            UnitsToPlay = 0
        });
        
        // Act
        _sut.ExecutePlayerAction();
        
        // Assert
        _commandPublisher.DidNotReceive().PublishCommand(Arg.Any<UpdatePlayerStatusCommand>());
    }

    [Fact]
    public void PlayerActionLabel_ReturnsCorrectLabel()
    {   
        // Act
        var result = _sut.PlayerActionLabel;
        
        // Assert
        result.ShouldBe("Ready to play");
    }
}
