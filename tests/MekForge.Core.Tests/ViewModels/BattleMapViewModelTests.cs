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
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units.Mechs;
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
        _viewModel.Turn.ShouldBe(1);
        _game.TurnPhase.Returns(PhaseNames.Start);
        _viewModel.TurnPhaseNames.ShouldBe(PhaseNames.Start);
        _game.ActivePlayer.Returns(new Player(Guid.Empty, "Player1", "#FF0000"));
        _viewModel.ActivePlayerName.ShouldBe("Player1");
        _viewModel.ActivePlayerTint.ShouldBe("#FF0000");
    }

    [Theory]
    [InlineData(1, "Select Unit",true)]
    [InlineData(0, "", false)]
    public void UnitsToDeploy_ShouldBeVisible_WhenItsPlayersTurnToDeploy_AndThereAreUnitsToDeploy(
        int unitsToMove, 
        string actionLabel,
        bool expectedVisible)
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var unitData = MechFactoryTests.CreateDummyMechData();

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
        
        // Assert
        if (expectedVisible)
        {
            _viewModel.UnitsToDeploy.ShouldHaveSingleItem();
        }
        else
        {
            _viewModel.UnitsToDeploy.ShouldBeEmpty();
        }
        _viewModel.AreUnitsToDeployVisible.ShouldBe(expectedVisible);
        _viewModel.UserActionLabel.ShouldBe(actionLabel);
        _viewModel.IsUserActionLabelVisible.ShouldBe(expectedVisible);
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
        units.Count.ShouldBe(3);
        units.ShouldContain(unit1);
        units.ShouldContain(unit2);
    }

    [Fact]
    public void CommandLog_ShouldBeEmpty_WhenGameIsNotClientGame()
    {
        // Assert
        _viewModel.CommandLog.ShouldBeEmpty();
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
        _viewModel.CommandLog.Count.ShouldBe(1);
        _viewModel.CommandLog.First().ShouldBeEquivalentTo(joinCommand.Format(_localizationService, clientGame));
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
        _viewModel.CommandLog.Count.ShouldBe(2);
        _viewModel.CommandLog.First().ShouldBeEquivalentTo(joinCommand.Format(_localizationService,clientGame));
        _viewModel.CommandLog.Last().ShouldBeEquivalentTo(phaseCommand.Format(_localizationService,clientGame));
    }

    [Fact]
    public void IsCommandLogExpanded_ShouldBeFalse_ByDefault()
    {
        // Assert
        _viewModel.IsCommandLogExpanded.ShouldBeFalse();
    }

    [Fact]
    public void ToggleCommandLog_ShouldToggleIsCommandLogExpanded()
    {
        // Arrange
        var propertyChangedEvents = new List<string>();
        _viewModel.PropertyChanged += (_, e) => propertyChangedEvents.Add(e.PropertyName ?? string.Empty);

        // Act & Assert - First toggle
        _viewModel.ToggleCommandLog();
        _viewModel.IsCommandLogExpanded.ShouldBeTrue();
        propertyChangedEvents.ShouldContain(nameof(BattleMapViewModel.IsCommandLogExpanded));

        // Clear events for second test
        propertyChangedEvents.Clear();

        // Act & Assert - Second toggle
        _viewModel.ToggleCommandLog();
        _viewModel.IsCommandLogExpanded.ShouldBeFalse();
        propertyChangedEvents.ShouldContain(nameof(BattleMapViewModel.IsCommandLogExpanded));
    }

    [Fact]
    public void MovementPhase_WithActivePlayer_ShouldShowCorrectActionLabel()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player], new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>());
        _viewModel.Game = _game;

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
        
        // Assert
        _viewModel.UserActionLabel.ShouldBe("Select unit to move");
        _viewModel.IsUserActionLabelVisible.ShouldBeTrue();
    }

    [Fact]
    public void ShowDirectionSelector_SetsPositionAndDirections()
    {
        // Arrange
        var position = new HexCoordinates(1, 1);
        var directions = new[] { HexDirection.Top, HexDirection.Bottom };

        // Act
        _viewModel.ShowDirectionSelector(position, directions);

        // Assert
        _viewModel.DirectionSelectorPosition.ShouldBe(position);
        _viewModel.IsDirectionSelectorVisible.ShouldBeTrue();
        _viewModel.AvailableDirections.ShouldBeEquivalentTo(directions);
    }

    [Fact]
    public void HideDirectionSelector_ClearsDirectionsAndVisibility()
    {
        // Arrange
        var position = new HexCoordinates(1, 1);
        var directions = new[] { HexDirection.Top, HexDirection.Bottom };
        _viewModel.ShowDirectionSelector(position, directions);

        // Act
        _viewModel.HideDirectionSelector();

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
        _viewModel.AvailableDirections.ShouldBeNull();
    }

    [Fact]
    public void IsRecordSheetButtonVisible_NoSelectedUnit_ReturnsFalse()
    {
        // Arrange
        _viewModel.SelectedUnit = null;
        _viewModel.IsRecordSheetExpanded = false;

        // Act & Assert
        _viewModel.IsRecordSheetButtonVisible.ShouldBeFalse();
    }
    
    [Fact]
    public void IsRecordSheetPanelVisible_NoSelectedUnit_ReturnsFalse()
    {
        // Arrange
        _viewModel.SelectedUnit = null;
        _viewModel.IsRecordSheetExpanded = false;

        // Act & Assert
        _viewModel.IsRecordSheetPanelVisible.ShouldBeFalse();
    }

    [Fact]
    public void IsRecordSheetButtonVisible_HasSelectedUnitButExpanded_ReturnsFalse()
    {
        // Arrange
        var unit = new Mech("Mech", "MK1",20,6,[]);
        _viewModel.SelectedUnit = unit;
        _viewModel.IsRecordSheetExpanded = true;

        // Act & Assert
        _viewModel.IsRecordSheetButtonVisible.ShouldBeFalse();
    }

    [Fact]
    public void IsRecordSheetButtonVisible_HasSelectedUnitNotExpanded_ReturnsTrue()
    {
        // Arrange
        var unit = new Mech("Mech", "MK1",20,6,[]);
        _viewModel.SelectedUnit = unit;
        _viewModel.IsRecordSheetExpanded = false;

        // Act & Assert
        _viewModel.IsRecordSheetButtonVisible.ShouldBeTrue();
    }
    
    [Fact]
    public void IsRecordSheetPanelVisible_HasSelectedUnitButExpanded_ReturnsTrue()
    {
        // Arrange
        var unit = new Mech("Mech", "MK1",20,6,[]);
        _viewModel.SelectedUnit = unit;
        _viewModel.IsRecordSheetExpanded = true;

        // Act & Assert
        _viewModel.IsRecordSheetPanelVisible.ShouldBeTrue();
    }

    [Fact]
    public void IsRecordSheetPanelVisible_HasSelectedUnitNotExpanded_ReturnsFalse()
    {
        // Arrange
        var unit = new Mech("Mech", "MK1",20,6,[]);
        _viewModel.SelectedUnit = unit;
        _viewModel.IsRecordSheetExpanded = false;

        // Act & Assert
        _viewModel.IsRecordSheetPanelVisible.ShouldBeFalse();
    }

    [Fact]
    public void ToggleRecordSheet_TogglesIsRecordSheetExpanded()
    {
        // Arrange
        _viewModel.IsRecordSheetExpanded = false;

        // Act
        _viewModel.ToggleRecordSheet();

        // Assert
        _viewModel.IsRecordSheetExpanded.ShouldBeTrue();

        // Act again
        _viewModel.ToggleRecordSheet();

        // Assert
        _viewModel.IsRecordSheetExpanded.ShouldBeFalse();
    }

    [Fact]
    public void ShowMovementPath_SetsMovementPathProperty()
    {
        // Arrange
        var path = new List<PathSegment>
        {
            new(
                new HexPosition(new HexCoordinates(1, 1), HexDirection.Top),
                new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom),
                1)
        };

        // Act
        _viewModel.ShowMovementPath(path);

        // Assert
        _viewModel.MovementPath.ShouldNotBeNull();
        _viewModel.MovementPath[0].From.ShouldBe(path[0].From);
        _viewModel.MovementPath[0].To.ShouldBe(path[0].To);
    }

    [Fact]
    public void ShowMovementPath_WithEmptyPath_ClearsMovementPath()
    {
        // Arrange
        var path = new List<PathSegment>
        {
            new(
                new HexPosition(new HexCoordinates(1, 1), HexDirection.Top),
                new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom),
                1)
        };
        _viewModel.ShowMovementPath(path);

        // Act
        _viewModel.ShowMovementPath([]);

        // Assert
        _viewModel.MovementPath.ShouldBeNull();
    }

    [Fact]
    public void ShowMovementPath_NotifiesPropertyChanged()
    {
        // Arrange
        var path = new List<PathSegment>
        {
            new(
                new HexPosition(new HexCoordinates(1, 1), HexDirection.Top),
                new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom),
                1)
        };
        var propertyChanged = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleMapViewModel.MovementPath))
                propertyChanged = true;
        };

        // Act
        _viewModel.ShowMovementPath(path);

        // Assert
        propertyChanged.ShouldBeTrue();
    }

    [Fact]
    public void HideMovementPath_ClearsPathAndNotifiesChange()
    {
        // Arrange
        var path = new List<PathSegment>
        {
            new(
                new HexPosition(new HexCoordinates(1, 1), HexDirection.Top),
                new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom),
                1)
        };
        _viewModel.ShowMovementPath(path);
        var propertyChanged = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BattleMapViewModel.MovementPath))
                propertyChanged = true;
        };

        // Act
        _viewModel.HideMovementPath();

        // Assert
        _viewModel.MovementPath.ShouldBeNull();
        propertyChanged.ShouldBeTrue();
    }
}