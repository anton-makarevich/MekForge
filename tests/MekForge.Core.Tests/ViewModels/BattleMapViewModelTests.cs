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
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.ViewModels;

public class BattleMapViewModelTests
{
    private readonly BattleMapViewModel _viewModel;
    private IGame _game;
    private readonly ILocalizationService _localizationService;

    public BattleMapViewModelTests()
    {
        var imageService = Substitute.For<IImageService>();
        
        _localizationService = Substitute.For<ILocalizationService>();
        _viewModel = new BattleMapViewModel(imageService, _localizationService);
        _game = Substitute.For<IGame>();
        _viewModel.Game = _game;
    }

    [Fact]
    public void GameUpdates_RaiseNotifyPropertyChanged()
    {

        // Act and Assert
        _game.Turn.Returns(1);
        _viewModel.Turn.Should().Be(1);
        _game.TurnPhase.Returns(PhaseNames.Start);
        _viewModel.TurnPhaseNames.Should().Be(PhaseNames.Start);
        _game.ActivePlayer.Returns(new Player(Guid.Empty, "Player1"));
        _viewModel.ActivePlayerName.Should().Be("Player1");
    }

    [Theory]
    [InlineData(1, "Select Unit",true)]
    [InlineData(0, "", false)]
    public async Task UnitsToDeploy_ShouldBeVisible_WhenItsPlayersTurnToDeploy_AndThereAreUnitsToDeploy(
        int unitsToMove, 
        string actionLabel,
        bool expectedVisible)
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();

        var tcs = new TaskCompletionSource<bool>();

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleMapViewModel.UnitsToDeploy))
            {
                tcs.SetResult(true); // Signal that the property has changed
            }
        };

        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player], new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = _game;

        ((ClientGame)_game).HandleCommand(new ChangePhaseCommand()
        {
            Phase = PhaseNames.Deployment,
            GameOriginId = Guid.NewGuid()
        });
        ((ClientGame)_game).HandleCommand(new JoinGameCommand()
        {
            PlayerId = player.Id,
            Units = [unitData],
            PlayerName = player.Name,
            GameOriginId = Guid.NewGuid(),
            Tint = "#FF0000"
        });

        // Act
        ((ClientGame)_game).HandleCommand(new ChangeActivePlayerCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            UnitsToPlay = unitsToMove
        });

        // Wait for the PropertyChanged event
        await tcs.Task;
        
        // Assert
        if (expectedVisible)
        {
            _viewModel.UnitsToDeploy.Should().ContainSingle();
        }
        else
        {
            _viewModel.UnitsToDeploy.Should().BeEmpty();
        }
        _viewModel.AreUnitsToDeployVisible.Should().Be(expectedVisible);
        _viewModel.UserActionLabel.Should().Be(actionLabel);
        _viewModel.IsUserActionLabelVisible.Should().Be(expectedVisible);
    }
    
    [Fact]
    public void Units_ReturnsAllUnitsFromPlayers()
    {
        // Arrange
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var player2 = new Player(Guid.NewGuid(), "Player2");

        var mechData = MechFactoryTests.CreateDummyMechData();
        var mechFactory = new MechFactory(new ClassicBattletechRulesProvider());
        var unit1 = mechFactory.Create(mechData); 
        var unit2 = mechFactory.Create(mechData); 
    
        player1.AddUnit(unit1);
        player2.AddUnit(unit1);
        player2.AddUnit(unit2);
    
        _game.Players.Returns(new List<Player> { player1, player2 });

        // Act
        var units = _viewModel.Units.ToList();

        // Assert
        units.Should().HaveCount(3);
        units.Should().Contain(unit1);
        units.Should().Contain(unit2);
    }

    [Fact]
    public void CommandLog_ShouldBeEmpty_WhenGameIsNotClientGame()
    {
        // Assert
        _viewModel.CommandLog.Should().BeEmpty();
    }

    [Fact]
    public void CommandLog_ShouldUpdateWithNewCommands_WhenClientGameReceivesCommands()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())) ,
            [player],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = clientGame;

        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player2",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        // Act
        clientGame.HandleCommand(joinCommand);

        // Assert
        _viewModel.CommandLog.Should().HaveCount(1);
        _viewModel.CommandLog.First().Should().BeEquivalentTo(joinCommand.Format(_localizationService, clientGame));
    }

    [Fact]
    public void CommandLog_ShouldPreserveCommandOrder_WhenMultipleCommandsReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())) ,
            [player],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = clientGame;

        var joinCommand = new JoinGameCommand
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Player2",
            GameOriginId = Guid.NewGuid(),
            Units = [],
            Tint = "#FF0000"
        };

        var phaseCommand = new ChangePhaseCommand
        {
            Phase = PhaseNames.Deployment,
            GameOriginId = Guid.NewGuid()
        };

        // Act
        clientGame.HandleCommand(joinCommand);
        clientGame.HandleCommand(phaseCommand);

        // Assert
        _viewModel.CommandLog.Should().HaveCount(2);
        _viewModel.CommandLog.First().Should().BeEquivalentTo(joinCommand.Format(_localizationService,clientGame));
        _viewModel.CommandLog.Last().Should().BeEquivalentTo(phaseCommand.Format(_localizationService,clientGame));
    }

    [Fact]
    public void IsCommandLogExpanded_ShouldBeFalse_ByDefault()
    {
        // Assert
        _viewModel.IsCommandLogExpanded.Should().BeFalse();
    }

    [Fact]
    public void ToggleCommandLog_ShouldToggleIsCommandLogExpanded()
    {
        // Arrange
        var propertyChangedEvents = new List<string>();
        _viewModel.PropertyChanged += (_, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act & Assert - First toggle
        _viewModel.ToggleCommandLog();
        _viewModel.IsCommandLogExpanded.Should().BeTrue();
        propertyChangedEvents.Should().Contain(nameof(BattleMapViewModel.IsCommandLogExpanded));

        // Clear events for second test
        propertyChangedEvents.Clear();

        // Act & Assert - Second toggle
        _viewModel.ToggleCommandLog();
        _viewModel.IsCommandLogExpanded.Should().BeFalse();
        propertyChangedEvents.Should().Contain(nameof(BattleMapViewModel.IsCommandLogExpanded));
    }

    [Fact]
    public async Task MovementPhase_WithActivePlayer_ShouldShowCorrectActionLabel()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player], new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = _game;
        var tcs = new TaskCompletionSource<bool>();

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleMapViewModel.UserActionLabel))
            {
                tcs.TrySetResult(true); // Signal that the property has changed
            }
        };

        ((ClientGame)_game).HandleCommand(new ChangePhaseCommand()
        {
            Phase = PhaseNames.Movement,
            GameOriginId = Guid.NewGuid()
        });
        ((ClientGame)_game).HandleCommand(new JoinGameCommand()
        {
            PlayerId = player.Id,
            Units = [],
            PlayerName = player.Name,
            GameOriginId = Guid.NewGuid(),
            Tint = "#FF0000"
        });
        
        // Act
        ((ClientGame)_game).HandleCommand(new ChangeActivePlayerCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            UnitsToPlay = 1
        });
        await tcs.Task;
        
        // Assert
        _viewModel.UserActionLabel.Should().Be("Select unit to move");
        _viewModel.IsUserActionLabelVisible.Should().BeTrue();
    }
}