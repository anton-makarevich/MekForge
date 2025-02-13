using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class WeaponsAttackState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private Unit? _selectedUnit;
    private readonly List<HexDirection> _availableDirections = [];
    private readonly Dictionary<Weapon, HashSet<HexCoordinates>> _weaponRanges = [];

    public WeaponsAttackStep CurrentStep { get; private set; } = WeaponsAttackStep.SelectingUnit;

    public string ActionLabel => CurrentStep switch
    {
        WeaponsAttackStep.SelectingUnit => "Select unit to fire weapons",
        WeaponsAttackStep.ActionSelection => "Select action",
        WeaponsAttackStep.WeaponsConfiguration => "Configure weapons",
        WeaponsAttackStep.TargetSelection => "Select target",
        _ => string.Empty
    };

    public bool IsActionRequired => true;

    public WeaponsAttackState(BattleMapViewModel viewModel)
    {
        _viewModel = viewModel;
        if (_viewModel.Game == null)
        {
            throw new InvalidOperationException("Game is null"); 
        }
        if (_viewModel.Game.ActivePlayer == null)
        {
            throw new InvalidOperationException("Active player is null"); 
        }
    }

    public void HandleUnitSelection(Unit? unit)
    {
        if (unit == null) return;
        if (unit.HasFiredWeapons) return;
        
        // Clear previous highlights if any
        if (_selectedUnit != null)
        {
            ClearWeaponRangeHighlights();
        }
        
        _selectedUnit = unit;
        CurrentStep = WeaponsAttackStep.ActionSelection;
        
        // Highlight weapon ranges for the newly selected unit
        HighlightWeaponRanges();
        
        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        if (HandleUnitSelectionFromHex(hex)) return;

        if (CurrentStep == WeaponsAttackStep.TargetSelection)
        {
            // Target selection will be implemented next
        }
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        if (CurrentStep != WeaponsAttackStep.WeaponsConfiguration 
            || _selectedUnit is not Mech mech 
            || !_availableDirections.Contains(direction)) return;
        
        _viewModel.HideDirectionSelector();
        
        // Send command to server
        var command = new WeaponConfigurationCommand
        {
            GameOriginId = _viewModel.Game!.Id,
            PlayerId = _viewModel.Game.ActivePlayer!.Id,
            UnitId = mech.Id,
            Configuration = new WeaponConfiguration
            {
                Type = WeaponConfigurationType.TorsoRotation,
                Value = (int)direction
            }
        };
        
        if (_viewModel.Game is ClientGame clientGame)
        {
            clientGame.ConfigureUnitWeapons(command);
        }

        // Return to action selection after rotation
        CurrentStep = WeaponsAttackStep.ActionSelection;
        _viewModel.NotifyStateChanged();
    }

    public void HandleTorsoRotation(Guid unitId)
    {
        if (_selectedUnit?.Id != unitId) return;
        ClearWeaponRangeHighlights();
        HighlightWeaponRanges();
    }

    private bool HandleUnitSelectionFromHex(Hex hex)
    {
        var unit = _viewModel.Units.FirstOrDefault(u => u.Position?.Coordinates == hex.Coordinates);
        if (unit == null 
            || unit == _selectedUnit
            || unit.HasFiredWeapons
            || unit.Owner?.Id != _viewModel.Game?.ActivePlayer?.Id) return false;
        
        ResetUnitSelection();
        _viewModel.SelectedUnit = unit;
        return true;
    }

    private void ResetUnitSelection()
    {
        if (_viewModel.SelectedUnit == null) return;
        
        ClearWeaponRangeHighlights();
        
        _viewModel.SelectedUnit = null;
        _selectedUnit = null;
        CurrentStep = WeaponsAttackStep.SelectingUnit;
        _viewModel.NotifyStateChanged();
    }

    public IEnumerable<StateAction> GetAvailableActions()
    {
        if (_selectedUnit == null || CurrentStep != WeaponsAttackStep.ActionSelection)
            return new List<StateAction>();

        var actions = new List<StateAction>();

        // Add torso rotation action if available
        if (_selectedUnit is Mech { CanRotateTorso: true } mech)
        {
            actions.Add(new StateAction(
                "Turn Torso",
                true,
                () => 
                {
                    UpdateAvailableDirections();
                    _viewModel.ShowDirectionSelector(mech.Position!.Value.Coordinates, _availableDirections);
                    CurrentStep = WeaponsAttackStep.WeaponsConfiguration;
                    _viewModel.NotifyStateChanged();
                }));
        }

        // Add target selection action
        actions.Add(new StateAction(
            "Select Target",
            true,
            () => 
            {
                CurrentStep = WeaponsAttackStep.TargetSelection;
                _viewModel.NotifyStateChanged();
            }));

        return actions;
    }

    private void UpdateAvailableDirections()
    {
        if (_selectedUnit is not Mech mech || mech.Position == null) return;
        
        var currentFacing = (int)mech.Position.Value.Facing;
        _availableDirections.Clear();

        // Add available directions based on PossibleTorsoRotation
        for (var i = 0; i < 6; i++)
        {
            var clockwiseSteps = (i - currentFacing + 6) % 6;
            var counterClockwiseSteps = (currentFacing - i + 6) % 6;
            var steps = Math.Min(clockwiseSteps, counterClockwiseSteps);

            if (steps <= mech.PossibleTorsoRotation && steps > 0)
            {
                _availableDirections.Add((HexDirection)i);
            }
        }
    }

    private void HighlightWeaponRanges()
    {
        if (_selectedUnit?.Position == null) return;

        var reachableHexes = new HashSet<HexCoordinates>();
        var unitPosition = _selectedUnit.Position.Value;
        _weaponRanges.Clear();

        foreach (var part in _selectedUnit.Parts)
        {
            var weapons = part.GetComponents<Weapon>();
            foreach (var weapon in weapons)
            {
                var maxRange = weapon.LongRange;
                var facing = part.Location switch
                {
                    PartLocation.LeftLeg or PartLocation.RightLeg => unitPosition.Facing,
                    _ => _selectedUnit is Mech mech ? mech.TorsoDirection : unitPosition.Facing
                };
                if (facing == null)
                {
                    continue;
                }

                var weaponHexes = new HashSet<HexCoordinates>();
                // For arms, we need to check both forward and side arcs
                if (part.Location is PartLocation.LeftArm or PartLocation.RightArm)
                {
                    var forwardHexes = unitPosition.Coordinates.GetHexesInFiringArc(facing.Value, FiringArc.Forward, maxRange);
                    var sideArc = part.Location == PartLocation.LeftArm ? FiringArc.Left : FiringArc.Right;
                    var sideHexes = unitPosition.Coordinates.GetHexesInFiringArc(facing.Value, sideArc, maxRange);
                    
                    weaponHexes.UnionWith(forwardHexes);
                    weaponHexes.UnionWith(sideHexes);
                }
                else
                {
                    // For torso, legs, and head weapons - only forward arc
                    var hexes = unitPosition.Coordinates.GetHexesInFiringArc(facing.Value, FiringArc.Forward, maxRange);
                    weaponHexes.UnionWith(hexes);
                }

                _weaponRanges[weapon] = weaponHexes;
                reachableHexes.UnionWith(weaponHexes);
            }
        }

        // Highlight the hexes
        _viewModel.HighlightHexes(reachableHexes.ToList(), true);
    }

    private void ClearWeaponRangeHighlights()
    {
        if (_selectedUnit?.Position == null) return;

        // Get all hexes in maximum weapon range and unhighlight them
        var maxRange = _selectedUnit.Parts
            .SelectMany(p => p.GetComponents<Weapon>())
            .Max(w => w.LongRange);

        var allPossibleHexes = _selectedUnit.Position.Value.Coordinates
            .GetCoordinatesInRange(maxRange);

        _weaponRanges.Clear();
        _viewModel.HighlightHexes(allPossibleHexes.ToList(), false);
    }

    /// <summary>
    /// Gets all weapons that can fire at a given hex coordinate
    /// </summary>
    /// <param name="target">The target hex coordinates</param>
    /// <returns>List of weapons that can fire at the target</returns>
    public IReadOnlyList<Weapon> GetWeaponsInRange(HexCoordinates target)
    {
        return _weaponRanges
            .Where(kvp => kvp.Value.Contains(target))
            .Select(kvp => kvp.Key)
            .ToList();
    }
}

public enum WeaponsAttackStep
{
    SelectingUnit,
    ActionSelection,
    WeaponsConfiguration,
    TargetSelection
}
