using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class WeaponsAttackState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private Unit? _selectedUnit;
    private List<HexDirection> _availableDirections = new List<HexDirection>();

    public WeaponsAttackStep CurrentStep { get; private set; } = WeaponsAttackStep.SelectingUnit;

    public string ActionLabel => CurrentStep switch
    {
        WeaponsAttackStep.SelectingUnit => "Select unit to fire weapons",
        WeaponsAttackStep.SelectingTarget => "Select target",
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
        
        _selectedUnit = unit;
        CurrentStep = WeaponsAttackStep.SelectingTarget;
        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        if (HandleUnitSelectionFromHex(hex)) return;
        // Target selection will be implemented next
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        if (_selectedUnit is not Mech mech || !_availableDirections.Contains(direction)) return;

        // Handle torso rotation
        mech.RotateTorso(direction);
        
        // Send command to server
        var command = new WeaponConfigurationCommand
        {
            GameOriginId = _viewModel.Game!.Id,
            PlayerId = _viewModel.Game.ActivePlayer!.Id,
            UnitId = mech.Id,
            TurretRotation = (int)direction
        };
        
        if (_viewModel.Game is ClientGame clientGame)
        {
            clientGame.ConfigureUnitWeapons(command);
        }
        
        _viewModel.NotifyStateChanged();
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
        _viewModel.SelectedUnit = null;
        _selectedUnit = null;
        CurrentStep = WeaponsAttackStep.SelectingUnit;
        _viewModel.NotifyStateChanged();
    }

    public IEnumerable<StateAction> GetAvailableActions()
    {
        if (_selectedUnit == null)
            return new List<StateAction>();

        var actions = new List<StateAction>();

        switch (CurrentStep)
        {
            case WeaponsAttackStep.SelectingTarget:
                if (_selectedUnit is Mech mech && mech.CanRotateTorso())
                {
                    actions.Add(new StateAction(
                        "Turn Torso",
                        true,
                        () => 
                        {
                            UpdateAvailableDirections();
                            _viewModel.ShowDirectionSelector(mech.Position!.Value.Coordinates, _availableDirections);
                            _viewModel.NotifyStateChanged();
                        }));
                }
                actions.Add(new StateAction(
                    "Select Target",
                    true,
                    () => 
                    {
                        CurrentStep = WeaponsAttackStep.SelectingTarget;
                        _viewModel.NotifyStateChanged();
                    }));
                break;
        }

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

    public List<HexDirection> GetDirectionAvailability() => _availableDirections;
}

public enum WeaponsAttackStep
{
    SelectingUnit,
    SelectingTarget
}
