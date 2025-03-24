using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data.Game;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data.Community;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils;
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
        _localizationService.GetString("Action_SelectUnitToDeploy").Returns("Select Unit");
        
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
        _viewModel.TurnPhaseName.ShouldBe(PhaseNames.Start);
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
        _viewModel.ActionInfoLabel.ShouldBe(actionLabel);
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
        _viewModel.ActionInfoLabel.ShouldBe("Select unit to move");
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
        _viewModel.ActionInfoLabel.ShouldBe("Select unit to fire weapons");
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
        _viewModel.ActionInfoLabel.ShouldBe("Wait");
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

    [Fact]
    public void WeaponAttacks_ShouldBePopulated_WhenWeaponAttackDeclarationCommandReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var targetPlayerId = Guid.NewGuid();
        var targetPlayer = new Player(targetPlayerId, "Player2");
        
        var mechData = MechFactoryTests.CreateDummyMechData();
        mechData.Id = Guid.NewGuid();
        // Create a game with the players
        var game = new ClientGame(
            BattleMap.GenerateMap(3, 3, new SingleTerrainGenerator(3, 3, new ClearTerrain())),
            [player, targetPlayer],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#ffffff",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        mechData.Id = Guid.NewGuid();
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [mechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        
        // Deploy units to positions
        var attackerPosition = new HexPosition(new HexCoordinates(2, 2), HexDirection.Top);
        var targetPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == playerId);
        var target = _viewModel.Units.First(u => u.Owner!.Id == targetPlayerId);
        attacker.Deploy(attackerPosition);
        target.Deploy(targetPosition);
        
        // Get a weapon from the attacker to use in the command
        var weapon = attacker.Parts.SelectMany(p => p.GetComponents<Weapon>()).First();
        
        // Create weapon target data
        var weaponTargetData = new WeaponTargetData
        {
            TargetId = target.Id,
            Weapon = new WeaponData
            {
                Location = weapon.MountedOn!.Location,
                Slots = weapon.MountedAtSlots,
                Name = weapon.Name
            },
            IsPrimaryTarget = true
        };
        
        // Create the weapon attack declaration command
        var weaponAttackCommand = new WeaponAttackDeclarationCommand
        {
            PlayerId = playerId,
            AttackerId = attacker.Id,
            WeaponTargets = [weaponTargetData],
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        game.HandleCommand(weaponAttackCommand);
        
        // Assert
        _viewModel.WeaponAttacks.ShouldNotBeNull();
        _viewModel.WeaponAttacks.Count.ShouldBe(1);
        
        var attack = _viewModel.WeaponAttacks.First();
        attack.From.ShouldBe(attackerPosition.Coordinates);
        attack.To.ShouldBe(targetPosition.Coordinates);
        attack.Weapon.ShouldBe(weapon);
        attack.AttackerTint.ShouldBe(player.Tint);
        attack.LineOffset.ShouldBe(5);
    }
    
    [Fact]
    public void WeaponAttacks_ShouldAccumulate_WhenMultipleWeaponAttackDeclarationCommandsReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var targetPlayerId = Guid.NewGuid();
        var targetPlayer = new Player(targetPlayerId, "Player2");
        
        var mechData = MechFactoryTests.CreateDummyMechData();
        mechData.Id = Guid.NewGuid();
        
        // Create a game with the players
        var game = new ClientGame(
            BattleMap.GenerateMap(3, 3, new SingleTerrainGenerator(3, 3, new ClearTerrain())),
            [player, targetPlayer],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        
        // Add units to the game via commands
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#ffffff",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        
        // Create a second attacker for the same player
        var mechData2 = MechFactoryTests.CreateDummyMechData();
        mechData2.Id = Guid.NewGuid();
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData2],
            Tint = "#ffffff",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        
        // Create target unit
        var targetMechData = MechFactoryTests.CreateDummyMechData();
        targetMechData.Id = Guid.NewGuid();
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [targetMechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        
        // Set player statuses
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        
        // Deploy units to positions
        var attacker1Position = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var attacker2Position = new HexPosition(new HexCoordinates(0, 1), HexDirection.Top);
        var targetPosition = new HexPosition(new HexCoordinates(1, 0), HexDirection.Top);
        
        // Get the units from the game
        var attackers = _viewModel.Units.Where(u => u.Owner!.Id == playerId).ToList();
        var attacker1 = attackers[0];
        var attacker2 = attackers[1];
        var target = _viewModel.Units.First(u => u.Owner!.Id == targetPlayerId);
        
        // Deploy the units
        attacker1.Deploy(attacker1Position);
        attacker2.Deploy(attacker2Position);
        target.Deploy(targetPosition);
        
        // Get weapons from the attackers
        var weapon1 = attacker1.Parts.SelectMany(p => p.GetComponents<Weapon>()).First();
        var weapon2 = attacker2.Parts.SelectMany(p => p.GetComponents<Weapon>()).First();
        
        // Create weapon target data
        var weaponTargetData1 = new WeaponTargetData
        {
            TargetId = target.Id,
            Weapon = new WeaponData
            {
                Location = weapon1.MountedOn!.Location,
                Slots = weapon1.MountedAtSlots,
                Name = weapon1.Name
            },
            IsPrimaryTarget = true
        };
        
        var weaponTargetData2 = new WeaponTargetData
        {
            TargetId = target.Id,
            Weapon = new WeaponData
            {
                Location = weapon2.MountedOn!.Location,
                Slots = weapon2.MountedAtSlots,
                Name = weapon2.Name
            },
            IsPrimaryTarget = true
        };
        
        // Create the weapon attack declaration commands
        var weaponAttackCommand1 = new WeaponAttackDeclarationCommand
        {
            PlayerId = playerId,
            AttackerId = attacker1.Id,
            WeaponTargets = [weaponTargetData1],
            GameOriginId = Guid.NewGuid()
        };
        
        var weaponAttackCommand2 = new WeaponAttackDeclarationCommand
        {
            PlayerId = playerId,
            AttackerId = attacker2.Id,
            WeaponTargets = [weaponTargetData2],
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        game.HandleCommand(weaponAttackCommand1);
        game.HandleCommand(weaponAttackCommand2);
        
        // Assert
        _viewModel.WeaponAttacks.ShouldNotBeNull();
        _viewModel.WeaponAttacks.Count.ShouldBe(2);
        
        var attack1 = _viewModel.WeaponAttacks[0];
        attack1.From.ShouldBe(attacker1Position.Coordinates);
        attack1.To.ShouldBe(targetPosition.Coordinates);
        attack1.Weapon.ShouldBe(weapon1);
        attack1.AttackerTint.ShouldBe(player.Tint);
        attack1.LineOffset.ShouldBe(5);
        
        var attack2 = _viewModel.WeaponAttacks[1];
        attack2.From.ShouldBe(attacker2Position.Coordinates);
        attack2.To.ShouldBe(targetPosition.Coordinates);
        attack2.Weapon.ShouldBe(weapon2);
        attack2.AttackerTint.ShouldBe(player.Tint);
        attack2.LineOffset.ShouldBe(5);
    }
    
    [Fact]
    public void WeaponAttacks_ShouldRemoveSpecificAttack_WhenWeaponAttackResolutionCommandReceived()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var targetPlayerId = Guid.NewGuid();
        var targetPlayer = new Player(targetPlayerId, "Player2");
        
        var mechData = MechFactoryTests.CreateDummyMechData();
        mechData.Id = Guid.NewGuid();
        
        // Create a game with the players
        var game = new ClientGame(
            BattleMap.GenerateMap(3, 3, new SingleTerrainGenerator(3, 3, new ClearTerrain())),
            [player, targetPlayer],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        
        _viewModel.Game = game;
        
        // Add units to the game via commands
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [mechData],
            Tint = "#ffffff",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        
        // Create target unit
        var targetMechData = MechFactoryTests.CreateDummyMechData();
        targetMechData.Id = Guid.NewGuid();
        game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [targetMechData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = targetPlayerId
        });
        
        // Deploy units to positions
        var attackerPosition = new HexPosition(new HexCoordinates(0, 0), HexDirection.Top);
        var targetPosition = new HexPosition(new HexCoordinates(1, 0), HexDirection.Top);
        
        // Get the units from the game
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == playerId);
        var target = _viewModel.Units.First(u => u.Owner!.Id == targetPlayerId);
        
        // Deploy the units
        attacker.Deploy(attackerPosition);
        target.Deploy(targetPosition);
        
        // Get two weapons from the attacker
        var weapons = attacker.Parts.SelectMany(p => p.GetComponents<Weapon>()).Take(2).ToList();
        var weapon1 = weapons[0];
        var weapon2 = weapons.Count > 1 ? weapons[1] : weapons[0]; // Fallback in case there's only one weapon
        
        // Create weapon target data for two weapons
        var weaponTargetData1 = new WeaponTargetData
        {
            TargetId = target.Id,
            Weapon = new WeaponData
            {
                Location = weapon1.MountedOn!.Location,
                Slots = weapon1.MountedAtSlots,
                Name = weapon1.Name
            },
            IsPrimaryTarget = true
        };
        
        var weaponTargetData2 = new WeaponTargetData
        {
            TargetId = target.Id,
            Weapon = new WeaponData
            {
                Location = weapon2.MountedOn!.Location,
                Slots = weapon2.MountedAtSlots,
                Name = weapon2.Name
            },
            IsPrimaryTarget = true
        };
        
        // Create and handle the weapon attack declaration command with two weapons
        var weaponAttackCommand = new WeaponAttackDeclarationCommand
        {
            PlayerId = playerId,
            AttackerId = attacker.Id,
            WeaponTargets = [weaponTargetData1, weaponTargetData2],
            GameOriginId = Guid.NewGuid()
        };
        
        game.HandleCommand(weaponAttackCommand);
        
        // Verify both attacks are present
        _viewModel.WeaponAttacks.ShouldNotBeNull();
        _viewModel.WeaponAttacks.Count.ShouldBe(2);
        
        // Create a resolution command for the first weapon
        var resolutionCommand = new WeaponAttackResolutionCommand
        {
            PlayerId = playerId,
            AttackerId = attacker.Id,
            TargetId = target.Id,
            WeaponData = weaponTargetData1.Weapon,
            ResolutionData = new AttackResolutionData(
                ToHitNumber: 7,
                AttackRoll: [new DiceResult(6)],
                IsHit: true),
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        game.HandleCommand(resolutionCommand);
        
        // Assert
        _viewModel.WeaponAttacks.Count.ShouldBe(1); // Only one attack should remain
        
        // The remaining attack should be for weapon2
        var remainingAttack = _viewModel.WeaponAttacks.First();
        remainingAttack.Weapon.ShouldBe(weapon2);
        remainingAttack.TargetId.ShouldBe(target.Id);
    }
    
    [Fact]
    public void UpdateGamePhase_ShouldTransitionToEndState_WhenPhaseIsEnd()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = clientGame;

        _localizationService.GetString("EndPhase_ActionLabel").Returns("End your turn");
        clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerName = player.Name,
            Units = [],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });

        // Act
        clientGame.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.End
        });
        // Set the player as active player
        clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            UnitsToPlay = 1
        });

        // Assert
        _viewModel.CurrentState.ShouldBeOfType<EndState>();
        _viewModel.ActionInfoLabel.ShouldBe("End your turn");
    }

    [Fact]
    public void IsPlayerActionButtonVisible_ShouldReturnTrue_WhenInEndPhaseWithActivePlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = clientGame;

        // Set up the game state for End phase
        clientGame.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.End
        });
        
        // Set the player as active player
        clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            UnitsToPlay = 0
        });

        // Assert
        _viewModel.IsPlayerActionButtonVisible.ShouldBeTrue();
    }

    [Fact]
    public void IsPlayerActionButtonVisible_ShouldReturnTrue_WhenInStartPhase()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player],
            new ClassicBattletechRulesProvider(),
            Substitute.For<ICommandPublisher>(),
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = clientGame;

        // Set up the game state for Start phase
        clientGame.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.Start
        });

        // Assert
        _viewModel.IsPlayerActionButtonVisible.ShouldBeTrue();
    }

    [Fact]
    public void IsPlayerActionButtonVisible_ShouldReturnFalse_WhenNotInEndOrStartPhase()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player1");
        var commandPublisher = Substitute.For<ICommandPublisher>();
        var clientGame = new ClientGame(
            BattleMap.GenerateMap(2, 2, new SingleTerrainGenerator(2, 2, new ClearTerrain())),
            [player],
            new ClassicBattletechRulesProvider(),
            commandPublisher,
            Substitute.For<IToHitCalculator>());
        _viewModel.Game = clientGame;

        _localizationService.GetString("EndPhase_ActionLabel").Returns("End your turn");
        clientGame.HandleCommand(new JoinGameCommand
        {
            PlayerName = player.Name,
            Units = [],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId
        });
        // Set up the game state for End phase
        clientGame.HandleCommand(new ChangePhaseCommand
        {
            GameOriginId = Guid.NewGuid(),
            Phase = PhaseNames.Movement
        });
        
        // Set the player as active player
        clientGame.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId,
            UnitsToPlay = 1
        });

        // Act
        _viewModel.HandlePlayerAction();

        // Assert
        _viewModel.IsPlayerActionButtonVisible.ShouldBeFalse();
    }
}