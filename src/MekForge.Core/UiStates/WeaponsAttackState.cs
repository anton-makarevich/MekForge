using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class WeaponsAttackState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private Unit? _selectedUnit;

    public WeaponsAttackStep CurrentStep { get; private set; } = WeaponsAttackStep.SelectingUnit;

    public string ActionLabel => CurrentStep switch
    {
        WeaponsAttackStep.SelectingUnit => "Select unit to fire weapons",
        WeaponsAttackStep.SelectingTarget => "Select target",
        WeaponsAttackStep.ConfiguringWeapons => "Configure weapons",
        WeaponsAttackStep.SelectingTorsoRotation => "Select torso rotation",
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
        if (CurrentStep != WeaponsAttackStep.SelectingTorsoRotation || _selectedUnit == null) return;

        // Handle torso rotation for the selected unit
        // This will be implemented when we add torso rotation functionality to Unit
        //_selectedUnit.SetTorsoRotation(direction);
        
        CurrentStep = WeaponsAttackStep.ConfiguringWeapons;
        _viewModel.NotifyStateChanged();
    }

    private bool HandleUnitSelectionFromHex(Hex hex)
    {
        var unit = _viewModel.Units.FirstOrDefault(u => u.Position?.Coordinates == hex.Coordinates);
        if (unit == null 
            || unit == _selectedUnit
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
}

public enum WeaponsAttackStep
{
    SelectingUnit,
    SelectingTarget,
    SelectingTorsoRotation,
    ConfiguringWeapons,
    Confirming
}
