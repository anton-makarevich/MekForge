using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
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
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    public BattleMapViewModelTests()
    {
        var imageService = Substitute.For<IImageService>();
        _viewModel = new BattleMapViewModel(imageService, _localizationService);
        
        _localizationService.GetString("Action_SelectTarget").Returns("Select Target");
        _localizationService.GetString("Action_SelectUnitToFire").Returns("Select unit to fire weapons");
        _localizationService.GetString("Action_SelectUnitToMove").Returns("Select unit to move");
        
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
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
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
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
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
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
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
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
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
    public void WeaponsAttackPhase_WithUnitsToPlay_ShouldShowCorrectActionLabel()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player], new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = _game;
        ((ClientGame)_game).HandleCommand(new JoinGameCommand()
        {
            PlayerId = player.Id,
            Units = [],
            PlayerName = player.Name,
            GameOriginId = Guid.NewGuid(),
            Tint = "#FF0000"
        });
        ((ClientGame)_game).HandleCommand(new ChangePhaseCommand()
        {
            Phase = PhaseNames.WeaponsAttack,
            GameOriginId = Guid.NewGuid()
        });
        
        
        // Act
        ((ClientGame)_game).HandleCommand(new ChangeActivePlayerCommand
        {
            PlayerId = player.Id,
            GameOriginId = Guid.NewGuid(),
            UnitsToPlay = 1
        });
        
        // Assert
        _viewModel.UserActionLabel.ShouldBe("Select unit to fire weapons");
        _viewModel.IsUserActionLabelVisible.ShouldBeTrue();
    }

    [Fact]
    public void WeaponsAttackPhase_WithNoUnitsToPlay_ShouldBeIdle()
    {
        // Arrange
        var player = new Player(Guid.NewGuid(), "Player1");
        _game = new ClientGame(BattleMap.GenerateMap(2, 2,
                new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player], new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = _game;

        ((ClientGame)_game).HandleCommand(new ChangePhaseCommand()
        {
            Phase = PhaseNames.WeaponsAttack,
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
            UnitsToPlay = 0
        });
        
        // Assert
        _viewModel.UserActionLabel.ShouldBe("Wait");
        _viewModel.IsUserActionLabelVisible.ShouldBeFalse();
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

    [Fact]
    public void WeaponSelectionItems_WhenNotInWeaponsAttackState_ReturnsEmptyList()
    {
        // Arrange
        _game.TurnPhase.Returns(PhaseNames.Movement);
        
        // Act
        var items = _viewModel.WeaponSelectionItems;

        // Assert
        items.ShouldBeEmpty();
    }

    [Fact]
    public void WeaponSelectionItems_WhenInWeaponsAttackState_ReturnsWeaponsFromState()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player = new Player(Guid.NewGuid(), "Player1");
        var game = new ClientGame(
            battleMap, [player], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id
        });
        game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.WeaponsAttack
        });
        game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });


        // Place unit
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        
        // Select unit
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==position.Coordinates));

        // Act
        var items = _viewModel.WeaponSelectionItems.ToList();

        // Assert
        items.ShouldNotBeEmpty();
        items.Count.ShouldBe(unit.Parts.Sum(p => p.GetComponents<Weapon>().Count()));
    }

    [Fact]
    public void IsWeaponSelectionVisible_WhenNotInWeaponsAttackState_ReturnsFalse()
    {
        // Arrange
        _game.TurnPhase.Returns(PhaseNames.Movement);
        
        // Act & Assert
        _viewModel.IsWeaponSelectionVisible.ShouldBeFalse();
    }

    [Fact]
    public void IsWeaponSelectionVisible_WhenInWeaponsAttackStateWithoutTarget_ReturnsFalse()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player = new Player(Guid.NewGuid(), "Player1");
        var game = new ClientGame(
            battleMap, [player], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id
        });
        game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player.Id,
            UnitsToPlay = 1
        });
        game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.WeaponsAttack
        });

        // Place unit
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        
        // Select unit
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==position.Coordinates));
        
        // Act & Assert
        _viewModel.IsWeaponSelectionVisible.ShouldBeFalse();
    }

    [Fact]
    public void IsWeaponSelectionVisible_WhenInWeaponsAttackStateWithTarget_ReturnsTrue()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var player2 = new Player(Guid.NewGuid(), "Player2");
        var game = new ClientGame(
            battleMap, [player1, player2], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [mechData],
            Tint = "#00FF00",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });
        game.HandleCommand(new ChangePhaseCommand
                   {
                       GameOriginId = Guid.NewGuid(),
                       Phase = PhaseNames.WeaponsAttack
                   });
        game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id,
            UnitsToPlay = 2
        });


        // Place units
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == player1.Id);
        attacker.Deploy(attackerPosition);
        
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        var target = _viewModel.Units.First(u => u.Owner!.Id == player2.Id);
        target.Deploy(targetPosition);
        
        // Select attacker
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        
        // Select target
        var selectTargetAction = _viewModel.CurrentState.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        
        // Act & Assert
        _viewModel.IsWeaponSelectionVisible.ShouldBeTrue();
    }

    [Fact]
    public void IsWeaponSelectionVisible_CanBeClosedAndReopened()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var player2 = new Player(Guid.NewGuid(), "Player2");
        var game = new ClientGame(
            battleMap, [player1, player2], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [mechData],
            Tint = "#00FF00",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player2.Id
        });
        game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.WeaponsAttack
        });
        game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id,
            UnitsToPlay = 1
        });
        
        // Place units
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == player1.Id);
        attacker.Deploy(attackerPosition);
        
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        var target = _viewModel.Units.First(u => u.Owner!.Id == player2.Id);
        target.Deploy(targetPosition);
        
        // Select attacker
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        
        // Select target
        var selectTargetAction = _viewModel.CurrentState.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        
        // Act & Assert - Initially visible
        _viewModel.IsWeaponSelectionVisible.ShouldBeTrue();

        // Act & Assert - Can be closed
        _viewModel.CloseWeaponSelectionCommand();
        _viewModel.IsWeaponSelectionVisible.ShouldBeFalse();

        // Act & Assert - Can be reopened
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        _viewModel.IsWeaponSelectionVisible.ShouldBeTrue();

        // Act & Assert - Stays closed when changing phase
        _viewModel.IsWeaponSelectionVisible = false;
        game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.Movement
        });
        _viewModel.IsWeaponSelectionVisible.ShouldBeFalse();
    }

    [Fact]
    public void Attacker_ShouldBeStateAttacker_DuringWeaponsAttack()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var game = new ClientGame(
            battleMap, [player1], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());

        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.WeaponsAttack
        });
        game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id,
            UnitsToPlay = 1
        });

        // Place units
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == player1.Id);
        attacker.Deploy(attackerPosition);

        // Act Select attacker
        _viewModel.HandleHexSelection(game.BattleMap.GetHexes()
            .First(h => h.Coordinates == attackerPosition.Coordinates));
        
        // Assert
        _viewModel.Attacker.ShouldBe(attacker);
    }
    
    [Fact]
    public void Attacker_ShouldBeNull_WhenNotInWeaponsAttack()
    {
        // Arrange
        var rules = new ClassicBattletechRulesProvider();
        var mechData = MechFactoryTests.CreateDummyMechData();
        var battleMap = BattleMap.GenerateMap(
            2, 11,
            new SingleTerrainGenerator(2, 11, new ClearTerrain()));
        var player1 = new Player(Guid.NewGuid(), "Player1");
        var game = new ClientGame(
            battleMap, [player1], rules,
            Substitute.For<ICommandPublisher>(), Substitute.For<IToHitCalculator>());

        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = player1.Id
        });
        
        // Assert
        _viewModel.Attacker.ShouldBeNull();
    }
}