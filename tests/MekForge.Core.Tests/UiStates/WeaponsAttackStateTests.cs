using Shouldly;
using NSubstitute;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.Utils;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class WeaponsAttackStateTests
{
    private readonly WeaponsAttackState _state;
    private readonly ClientGame _game;
    private readonly UnitData _unitData;
    private readonly Unit _unit1;
    private readonly Unit _unit2;
    private readonly Player _player;
    private readonly BattleMapViewModel _viewModel;
    private readonly IToHitCalculator _toHitCalculator = Substitute.For<IToHitCalculator>();

    public WeaponsAttackStateTests()
    {
        var imageService = Substitute.For<IImageService>();
        var localizationService = Substitute.For<ILocalizationService>();
        _viewModel = new BattleMapViewModel(imageService, localizationService);
        var playerId = Guid.NewGuid();

        var rules = new ClassicBattletechRulesProvider();
        _unitData = MechFactoryTests.CreateDummyMechData();
        _unit1 = new MechFactory(rules).Create(_unitData);
        _unit2 = new MechFactory(rules).Create(_unitData);

        var battleMap = BattleMap.GenerateMap(
            11, 11,
            new SingleTerrainGenerator(11, 11, new ClearTerrain()));
        _player = new Player(playerId, "Player1");
        _game = new ClientGame(
            battleMap, [_player], rules,
            Substitute.For<ICommandPublisher>(), _toHitCalculator);

        var expectedModifiers = new ToHitBreakdown
        {
            GunneryBase = new GunneryAttackModifier { Value = 4 },
            AttackerMovement = new AttackerMovementModifier { Value = 0, MovementType = MovementType.StandingStill },
            TargetMovement = new TargetMovementModifier { Value = 0, HexesMoved = 1 },
            OtherModifiers = [],
            RangeModifier = new RangeAttackModifier
                { Value = 0, Range = WeaponRange.Medium, Distance = 5, WeaponName = "Test" },
            TerrainModifiers = [],
            HasLineOfSight = true
        };
        
        _toHitCalculator.GetModifierBreakdown(
                Arg.Any<Unit>(), 
                Arg.Any<Unit>(), 
                Arg.Any<Weapon>(), 
                Arg.Any<BattleMap>(),
                Arg.Any<bool>())
            .Returns(expectedModifiers);
        
        _viewModel.Game = _game;
        AddPlayerUnits();
        SetActivePlayer();
        _state = new WeaponsAttackState(_viewModel);
    }

    private void AddPlayerUnits()
    {
        var playerId2 = Guid.NewGuid();
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player1",
            Units = [_unitData, _unitData],
            Tint = "#FF0000",
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id
        });
        _game.HandleCommand(new JoinGameCommand
        {
            PlayerName = "Player2",
            Units = [_unitData,_unitData],
            Tint = "#FFFF00",
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId2
        });
        _game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id
        });
        _game.HandleCommand(new UpdatePlayerStatusCommand
        {
            PlayerStatus = PlayerStatus.Playing,
            GameOriginId = Guid.NewGuid(),
            PlayerId = playerId2
        });
    }

    private void SetActivePlayer()
    {
        _game.HandleCommand(new ChangeActivePlayerCommand
        {
            GameOriginId = Guid.NewGuid(),
            PlayerId = _player.Id,
            UnitsToPlay = 1
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

    [Fact]
    public void InitialState_HasSelectUnitAction()
    {
        // Assert
        _state.ActionLabel.ShouldBe("Select unit to fire weapons");
        _state.IsActionRequired.ShouldBeTrue();
    }

    [Fact]
    public void HandleUnitSelection_TransitionsToActionSelection()
    {
        // Act
        _state.HandleUnitSelection(_unit1);

        // Assert
        _state.ActionLabel.ShouldBe("Select action");
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.ActionSelection);
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
    }

    [Fact]
    public void HandleHexSelection_SelectsUnit_WhenUnitIsOnHex_AndOtherUnitIsSelected()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        var hex = new Hex(position.Coordinates);
        _viewModel.SelectedUnit = _unit2;

        // Act
        _state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBe(unit);
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectUnit_WhenUnitHasFiredWeapons()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        var state = _viewModel.CurrentState;
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unit = _viewModel.Units.First();
        unit.Deploy(position);
        unit.FireWeapons();
        var hex = new Hex(position.Coordinates);

        // Act
        state.HandleHexSelection(hex);

        // Assert
        _viewModel.SelectedUnit.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_SelectsEnemyTarget_WhenInTargetSelectionStep_AndInRange()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        
        // Setup attacker (player's unit)
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        attacker.Deploy(attackerPosition);
        _state.HandleUnitSelection(attacker);
        
        // Setup target (enemy unit)
        var targetPosition = new HexPosition(new HexCoordinates(2, 1), HexDirection.Bottom);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        target.Deploy(targetPosition);
        
        // Activate target selection
        var targetSelectionAction = _state.GetAvailableActions()
            .First(a => a.Label == "Select Target");
        targetSelectionAction.OnExecute();
        
        // Create hex with enemy unit
        var targetHex = new Hex(targetPosition.Coordinates);

        // Act
        _state.HandleHexSelection(targetHex);
        _state.HandleUnitSelection(target);

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.TargetSelection);
        _viewModel.SelectedUnit.ShouldBe(target);
        _state.Attacker.ShouldBe(attacker);
        _state.SelectedTarget.ShouldBe(target);
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectFriendlyUnit_WhenInTargetSelectionStep()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        
        // Setup attacker (player's unit)
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        attacker.Deploy(attackerPosition);
        _state.HandleUnitSelection(attacker);
        
        // Setup another friendly unit
        var friendlyPosition = new HexPosition(new HexCoordinates(2, 1), HexDirection.Bottom);
        var friendlyUnit = _viewModel.Units.First(u => u.Owner!.Id == _player.Id && u != attacker);
        friendlyUnit.Deploy(friendlyPosition);
        
        // Activate target selection
        var targetSelectionAction = _state.GetAvailableActions()
            .First(a => a.Label == "Select Target");
        targetSelectionAction.OnExecute();
        
        // Create hex with friendly unit
        var friendlyHex = new Hex(friendlyPosition.Coordinates);

        // Act
        _state.HandleHexSelection(friendlyHex);

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.TargetSelection);
        _viewModel.SelectedUnit.ShouldBeNull();
        _state.Attacker.ShouldBe(attacker);
        _state.SelectedTarget.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectEnemyUnit_WhenNotInTargetSelectionStep()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        
        // Setup enemy unit
        var enemyPosition = new HexPosition(new HexCoordinates(2, 1), HexDirection.Bottom);
        var enemyUnit = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        enemyUnit.Deploy(enemyPosition);
        
        // Create hex with enemy unit
        var enemyHex = new Hex(enemyPosition.Coordinates);

        // Act
        _state.HandleHexSelection(enemyHex);

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.SelectingUnit);
        _viewModel.SelectedUnit.ShouldBeNull();
        _state.SelectedTarget.ShouldBeNull();
    }

    [Fact]
    public void HandleHexSelection_DoesNotSelectEnemyUnit_WhenOutOfWeaponRange()
    {
        // Arrange
        SetPhase(PhaseNames.WeaponsAttack);
        SetActivePlayer();
        
        // Setup attacker (player's unit)
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        attacker.Deploy(attackerPosition);
        _state.HandleUnitSelection(attacker);
        
        // Setup enemy unit far away (out of weapon range)
        var enemyPosition = new HexPosition(new HexCoordinates(10, 10), HexDirection.Bottom);
        var enemyUnit = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        enemyUnit.Deploy(enemyPosition);
        
        // Activate target selection
        var targetSelectionAction = _state.GetAvailableActions()
            .First(a => a.Label == "Select Target");
        targetSelectionAction.OnExecute();
        
        // Create hex with enemy unit
        var enemyHex = new Hex(enemyPosition.Coordinates);

        // Act
        _state.HandleHexSelection(enemyHex);

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.TargetSelection);
        _viewModel.SelectedUnit.ShouldBeNull();
        _state.Attacker.ShouldBe(attacker);
        _state.SelectedTarget.ShouldBeNull();
    }

    [Fact]
    public void GetAvailableActions_NoSelectedUnit_ReturnsEmpty()
    {
        // Act
        var actions = _state.GetAvailableActions();

        // Assert
        actions.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailableActions_NotInActionSelectionStep_ReturnsEmpty()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1, 1, HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        IEnumerable<StateAction> actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");
        torsoAction.OnExecute(); // This puts us in WeaponsConfiguration step

        // Act
        actions = _state.GetAvailableActions();

        // Assert
        actions.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailableActions_InActionSelection_ReturnsTorsoAndTargetOptions()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);

        // Act
        var actions = _state.GetAvailableActions().ToList();

        // Assert
        actions.Count.ShouldBe(2);
        actions[0].Label.ShouldBe("Turn Torso");
        actions[1].Label.ShouldBe("Select Target");
    }

    [Fact]
    public void GetAvailableActions_TorsoRotationAction_TransitionsToWeaponsConfiguration()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1, 1, HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");

        // Act
        torsoAction.OnExecute();

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.WeaponsConfiguration);
        _state.ActionLabel.ShouldBe("Configure weapons");
    }

    [Fact]
    public void GetAvailableActions_SelectTargetAction_TransitionsToTargetSelection()
    {
        // Arrange
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var selectTargetAction = actions.First(a => a.Label == "Select Target");

        // Act
        selectTargetAction.OnExecute();

        // Assert
        _state.CurrentStep.ShouldBe(WeaponsAttackStep.TargetSelection);
        _state.ActionLabel.ShouldBe("Select target");
    }

    [Fact]
    public void HandleFacingSelection_HidesDirectionSelector()
    {
        // Arrange
        _unit1.Deploy(new HexPosition(1, 1, HexDirection.Bottom));
        _state.HandleUnitSelection(_unit1);
        var actions = _state.GetAvailableActions().ToList();
        var torsoAction = actions.First(a => a.Label == "Turn Torso");
        torsoAction.OnExecute();

        // Act
        _state.HandleFacingSelection(HexDirection.BottomLeft);

        // Assert
        _viewModel.IsDirectionSelectorVisible.ShouldBeFalse();
        _state.ActionLabel.ShouldBe("Select action");
    }

    [Fact]
    public void HandleUnitSelection_HighlightsWeaponRanges_WhenUnitIsSelected()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        
        // Act
        _state.HandleUnitSelection(_unit1);

        // Assert
        var highlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        highlightedHexes.ShouldNotBeEmpty();
    }

    [Fact]
    public void HandleUnitSelection_ClearsPreviousHighlights_WhenSelectingNewUnit()
    {
        // Arrange
        var position1 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var position2 = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        var unit1= _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var unit2 = _viewModel.Units.Last(u => u.Owner!.Id == _player.Id);
        unit1.Deploy(position1);
        unit2.Deploy(position2);
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==position1.Coordinates));
        _state.HandleUnitSelection(unit1);
        var firstHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();

        // Act
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==position2.Coordinates));
        _state.HandleUnitSelection(unit2);

        // Assert
        var secondHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        secondHighlightedHexes.ShouldNotBeEmpty();
        // Check that at least some hexes are different (since units are in different positions)
        secondHighlightedHexes.ShouldNotBe(firstHighlightedHexes);
    }

    [Fact]
    public void ResetUnitSelection_ClearsWeaponRangeHighlights()
    {
        // Arrange
        var position1 = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var position2 = new HexPosition(new HexCoordinates(2, 2), HexDirection.Bottom);
        _unit1.Deploy(position1);
        _unit2.Deploy(position2);
        
        // Act
        _state.HandleUnitSelection(_unit1);
        var firstUnitHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        _state.HandleUnitSelection(_unit2);
        var secondUnitHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();

        // Assert
        firstUnitHighlightedHexes.ShouldNotBeEmpty();
        secondUnitHighlightedHexes.ShouldNotBeEmpty();
        secondUnitHighlightedHexes.ShouldNotBe(firstUnitHighlightedHexes);
    }

    [Fact]
    public void HandleTorsoRotation_UpdatesWeaponRangeHighlights()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        var firstHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        
        // Act
        ((Mech)_unit1).RotateTorso(HexDirection.BottomLeft);
        _state.HandleTorsoRotation(_unit1.Id);
        var secondHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();

        // Assert
        firstHighlightedHexes.ShouldNotBeEmpty();
        secondHighlightedHexes.ShouldNotBeEmpty();
        // Since we rotated the torso, the highlighted hexes should be different
        secondHighlightedHexes.ShouldNotBe(firstHighlightedHexes);
    }

    [Fact]
    public void HandleTorsoRotation_DoesNotUpdateHighlights_WhenDifferentUnit()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        var firstHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        
        // Act
        ((Mech)_unit1).RotateTorso(HexDirection.BottomLeft);
        _state.HandleTorsoRotation(Guid.NewGuid()); // Different unit ID
        var secondHighlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();

        // Assert
        firstHighlightedHexes.ShouldNotBeEmpty();
        secondHighlightedHexes.ShouldNotBeEmpty();
        // Since we tried to rotate a different unit's torso, the highlighted hexes should remain the same
        secondHighlightedHexes.ShouldBe(firstHighlightedHexes);
    }

    [Fact]
    public void HandleUnitSelection_DoesNotHighlightRanges_WhenUnitHasNoWeapons()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unitDataNoWeapons = MechFactoryTests.CreateDummyMechData();
        // Remove all weapons from the unit
        foreach (var (_, equipment) in unitDataNoWeapons.LocationEquipment)
        {
            equipment.RemoveAll(e => e is MekForgeComponent.MachineGun 
                or MekForgeComponent.SmallLaser
                or MekForgeComponent.MediumLaser
                or MekForgeComponent.LargeLaser
                or MekForgeComponent.PPC
                or MekForgeComponent.LRM5
                or MekForgeComponent.LRM10
                or MekForgeComponent.LRM15
                or MekForgeComponent.LRM20
                or MekForgeComponent.SRM2
                or MekForgeComponent.SRM4
                or MekForgeComponent.SRM6
                or MekForgeComponent.AC2
                or MekForgeComponent.AC5
                or MekForgeComponent.AC10
                or MekForgeComponent.AC20);
        }
        var unitNoWeapons = new MechFactory(new ClassicBattletechRulesProvider()).Create(unitDataNoWeapons);
        unitNoWeapons.Deploy(position);

        // Act
        _state.HandleUnitSelection(unitNoWeapons);

        // Assert
        var highlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        highlightedHexes.ShouldBeEmpty();
    }

    [Fact]
    public void HandleUnitSelection_HighlightsWeaponRanges_FromDifferentLocations()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var unitData = MechFactoryTests.CreateDummyMechData();
        
        // Initialize all locations with empty lists if they don't exist
        foreach (PartLocation location in Enum.GetValues(typeof(PartLocation)))
        {
            if (!unitData.LocationEquipment.ContainsKey(location))
                unitData.LocationEquipment[location] = new List<MekForgeComponent>();
        }
        
        // First remove all weapons
        foreach (var (_, equipment) in unitData.LocationEquipment)
        {
            equipment.RemoveAll(e => e is MekForgeComponent.MachineGun 
                or MekForgeComponent.SmallLaser
                or MekForgeComponent.MediumLaser
                or MekForgeComponent.LargeLaser
                or MekForgeComponent.PPC
                or MekForgeComponent.LRM5
                or MekForgeComponent.LRM10
                or MekForgeComponent.LRM15
                or MekForgeComponent.LRM20
                or MekForgeComponent.SRM2
                or MekForgeComponent.SRM4
                or MekForgeComponent.SRM6
                or MekForgeComponent.AC2
                or MekForgeComponent.AC5
                or MekForgeComponent.AC10
                or MekForgeComponent.AC20);
        }
        
        // Add weapons to different locations
        unitData.LocationEquipment[PartLocation.LeftTorso].Add(MekForgeComponent.LRM5);
        unitData.LocationEquipment[PartLocation.RightTorso].Add(MekForgeComponent.MediumLaser);
        unitData.LocationEquipment[PartLocation.CenterTorso].Add(MekForgeComponent.MediumLaser);
        unitData.LocationEquipment[PartLocation.LeftLeg].Add(MekForgeComponent.MediumLaser);
        unitData.LocationEquipment[PartLocation.RightLeg].Add(MekForgeComponent.MediumLaser);

        var unit = new MechFactory(new ClassicBattletechRulesProvider()).Create(unitData);
        unit.Deploy(position);

        // Act
        _state.HandleUnitSelection(unit);

        // Assert
        var highlightedHexes = _game.BattleMap.GetHexes().Where(h => h.IsHighlighted).ToList();
        highlightedHexes.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetWeaponsInRange_ReturnsEmptyList_WhenNoUnitSelected()
    {
        // Arrange
        var targetCoordinates = new HexCoordinates(2, 2);

        // Act
        var weapons = _state.GetWeaponsInRange(targetCoordinates);

        // Assert
        weapons.ShouldBeEmpty();
    }

    [Fact]
    public void GetWeaponsInRange_ReturnsWeaponsInRange_WhenTargetInForwardArc()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        var targetCoordinates = new HexCoordinates(1, 3); // Two hexes directly in front

        // Act
        var weapons = _state.GetWeaponsInRange(targetCoordinates);

        // Assert
        weapons.ShouldNotBeEmpty();
        // All weapons from torso and arms should be able to fire forward
        weapons.Count.ShouldBe(_unit1.Parts
            .Where(p => p.Location is PartLocation.LeftArm or PartLocation.RightArm or PartLocation.CenterTorso)
            .SelectMany(p => p.GetComponents<Weapon>())
            .Count());
    }

    [Fact]
    public void GetWeaponsInRange_ReturnsNoWeapons_WhenTargetOutOfRange()
    {
        // Arrange
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        _unit1.Deploy(position);
        _state.HandleUnitSelection(_unit1);
        // Get maximum weapon range
        var maxRange = _unit1.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .Max(w => w.LongRange);
        var targetCoordinates = new HexCoordinates(1, maxRange + 2); // Beyond maximum range

        // Act
        var weapons = _state.GetWeaponsInRange(targetCoordinates);

        // Assert
        weapons.ShouldBeEmpty();
    }

    [Fact]
    public void GetWeaponSelectionItems_WhenNoAttackerOrTarget_ReturnsEmptyList()
    {
        // Act
        var items = _state.GetWeaponSelectionItems();

        // Assert
        items.ShouldBeEmpty();
    }

    [Fact]
    public void GetWeaponSelectionItems_WhenAttackerSelected_CreatesViewModelsForAllWeapons()
    {
        // Arrange
        var unit = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var position = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        unit.Deploy(position);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==position.Coordinates));
        _state.HandleUnitSelection(unit);

        // Act
        var items = _state.GetWeaponSelectionItems().ToList();

        // Assert
        items.ShouldNotBeEmpty();
        items.Count.ShouldBe(unit.Parts.Sum(p => p.GetComponents<Weapon>().Count()));
        items.All(i => i.IsEnabled == false).ShouldBeTrue();
        items.All(i => i.IsSelected == false).ShouldBeTrue();
        items.All(i => i.Target == null).ShouldBeTrue();
    }
    
    [Fact]
    public void GetWeaponSelectionItems_WhenTargetIsNotSelected_UpdatesAvailabilityBasedOnRange()
    {
        // Arrange
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        
        // Place units next to each other
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        attacker.Deploy(attackerPosition);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);

        // Act
        var item = _state.GetWeaponSelectionItems().First();
        item.IsSelected = true;

        // Assert
        item.IsSelected.ShouldBeFalse();
        item.Target.ShouldBeNull();
    }

    [Fact]
    public void GetWeaponSelectionItems_WhenTargetSelected_UpdatesAvailabilityBasedOnRange()
    {
        // Arrange
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        
        // Place units next to each other
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        attacker.Deploy(attackerPosition);
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        target.Deploy(targetPosition);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        _state.HandleUnitSelection(target);

        // Act
        var items = _state.GetWeaponSelectionItems().ToList();

        // Assert
        items.ShouldNotBeEmpty();
        items.Any(i => i.IsEnabled).ShouldBeTrue(); // At least one weapon should be in range
        items.All(i => i.IsSelected == false).ShouldBeTrue();
        items.All(i => i.Target == null).ShouldBeTrue();
    }

    [Fact]
    public void HandleWeaponSelection_WhenWeaponSelected_AssignsTargetAndUpdatesViewModel()
    {
        // Arrange
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        
        // Place units next to each other
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        attacker.Deploy(attackerPosition);
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        target.Deploy(targetPosition);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        _state.HandleUnitSelection(target);

        var weapon = attacker.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .First();
        var weaponSelection = _state.GetWeaponSelectionItems().First(ws => ws.Weapon == weapon);

        // Act
        weaponSelection.IsSelected = true;

        // Assert
        weaponSelection.IsSelected.ShouldBeTrue();
        weaponSelection.Target.ShouldBe(target);
    }

    [Fact]
    public void HandleWeaponSelection_WhenWeaponDeselected_RemovesTargetAndUpdatesViewModel()
    {
        // Arrange
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        
        // Place units next to each other
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        attacker.Deploy(attackerPosition);
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        target.Deploy(targetPosition);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==targetPosition.Coordinates));
        _state.HandleUnitSelection(target);

        var weapon = attacker.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .First();
        var weaponSelection = _state.GetWeaponSelectionItems().First(i => i.Weapon == weapon);

        // Select weapon first
        weaponSelection.IsSelected=true;

        // Act
        weaponSelection.IsSelected=false;

        // Assert
        weaponSelection.IsSelected.ShouldBeFalse();
        weaponSelection.Target.ShouldBeNull();
    }

    [Fact]
    public void HandleWeaponSelection_WhenWeaponSelectedForDifferentTarget_DisablesWeaponForCurrentTarget()
    {
        // Arrange
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target1 = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        var target2 = _viewModel.Units.Last(u => u.Owner!.Id != _player.Id);
        
        // Place units in a triangle
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        attacker.Deploy(attackerPosition);
        var target1Position = new HexPosition(new HexCoordinates(1, 2), HexDirection.Bottom);
        target1.Deploy(target1Position);
        var target2Position = new HexPosition(new HexCoordinates(1, 3), HexDirection.Bottom);
        target2.Deploy(target2Position);
        
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==target1Position.Coordinates));
        _state.HandleUnitSelection(target1);

        var weapon = attacker.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .First();

        // Select weapon for first target
        var weaponSelection = _state.GetWeaponSelectionItems().First(ws => ws.Weapon == weapon);
        weaponSelection.IsSelected = true;

        // Act - select second target
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h=>h.Coordinates==target2Position.Coordinates));
        _state.HandleUnitSelection(target2);

        // Assert
        weaponSelection.IsEnabled.ShouldBeFalse(); // Should be disabled because it's targeting target1
        weaponSelection.Target.ShouldBe(target1);
    }

    [Fact]
    public void UpdateWeaponViewModels_CalculatesHitProbability_ForWeaponsInRange()
    {
        // Arrange
        // Set up attacker and target units
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        
        // Position units on the map
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var targetPosition = new HexPosition(new HexCoordinates(1, 2), HexDirection.Top);
        attacker.Deploy(attackerPosition);
        target.Deploy(targetPosition);
        
        // Select attacker and target
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h => h.Coordinates == attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h => h.Coordinates == targetPosition.Coordinates));
        _state.HandleUnitSelection(target);
        
        // Act
        var weaponItems = _state.GetWeaponSelectionItems().ToList();
        
        // Assert
        weaponItems.ShouldNotBeEmpty();
        foreach (var item in weaponItems.Where(i => i.IsInRange))
        {
            // Expected probability for target number 4 is 92%
            var expectedProbability = DiceUtils.Calculate2d6Probability(item.ModifiersBreakdown!.Total);
            item.HitProbability.ShouldBeEquivalentTo(expectedProbability);
            item.HitProbabilityText.ShouldBe($"{expectedProbability:F0}%");
        }
    }
    
    [Fact]
    public void UpdateWeaponViewModels_SetsNAForHitProbability_WhenWeaponNotInRange()
    {
        // Arrange
        // Set up attacker and target units
        var attacker = _viewModel.Units.First(u => u.Owner!.Id == _player.Id);
        var target = _viewModel.Units.First(u => u.Owner!.Id != _player.Id);
        
        // Position units on the map - far apart to ensure weapons are out of range
        var attackerPosition = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var targetPosition = new HexPosition(new HexCoordinates(10, 10), HexDirection.Top);
        attacker.Deploy(attackerPosition);
        target.Deploy(targetPosition);
        
        // Select attacker and target
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h => h.Coordinates == attackerPosition.Coordinates));
        _state.HandleUnitSelection(attacker);
        var selectTargetAction = _state.GetAvailableActions().First(a => a.Label == "Select Target");
        selectTargetAction.OnExecute();
        _state.HandleHexSelection(_game.BattleMap.GetHexes().First(h => h.Coordinates == targetPosition.Coordinates));
        _state.HandleUnitSelection(target);
        
        // Act
        var weaponItems = _state.GetWeaponSelectionItems().ToList();
        
        // Assert
        weaponItems.ShouldNotBeEmpty();
        foreach (var item in weaponItems)
        {
            // All weapons should be out of range
            item.IsInRange.ShouldBeFalse();
            item.HitProbability.ShouldBeEquivalentTo(0.0);
            item.HitProbabilityText.ShouldBe("N/A");
        }
    }
}
