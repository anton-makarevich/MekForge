using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class WeaponsAttackState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private readonly WeaponsAttackCommandBuilder _builder;
    private Unit? _selectedUnit;

    public WeaponsAttackStep CurrentStep { get; private set; } = WeaponsAttackStep.SelectingUnit;

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
        _builder = new WeaponsAttackCommandBuilder(_viewModel.Game.Id, _viewModel.Game.ActivePlayer.Id);
    }

    public void HandleUnitSelection(Unit? unit)
    {
        if (unit == null) return;
        if (unit.HasAttacked) return;
        
        _selectedUnit = unit;
        _builder.SetUnit(unit);
        CurrentStep = WeaponsAttackStep.SelectingTarget;
        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        if (HandleUnitSelectionFromHex(hex)) return;
        // Target selection will be implemented next
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
    ConfiguringWeapons,
    Confirming
}
