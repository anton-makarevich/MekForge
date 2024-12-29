using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MekForge.Core.ViewModels.States;

namespace Sanet.MekForge.Core.UiStates;

public class DeploymentState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private readonly DeploymentCommandBuilder _builder;
    private Hex? _selectedHex;
    
    private enum SubState
    {
        SelectingUnit,
        SelectingHex,
        SelectingDirection
    }
    
    private SubState _currentSubState = SubState.SelectingUnit;

    public DeploymentState(BattleMapViewModel viewModel, DeploymentCommandBuilder builder)
    {
        _viewModel = viewModel;
        _builder = builder;
    }

    public void HandleUnitSelection(Unit? unit)
    {
        if (_currentSubState != SubState.SelectingUnit) return;
        
        if (unit == null) return;
        
        _builder.SetUnit(unit);
        _currentSubState = SubState.SelectingHex;
        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        switch (_currentSubState)
        {
            case SubState.SelectingHex:
                HandleHexForDeployment(hex);
                break;
            case SubState.SelectingDirection:
                HandleHexForDirection(hex);
                break;
        }
    }

    private void HandleHexForDeployment(Hex hex)
    {
        _selectedHex = hex;
        _builder.SetPosition(hex.Coordinates);
        _currentSubState = SubState.SelectingDirection;
        
        var adjacentCoordinates = hex.Coordinates.GetAdjacentCoordinates().ToList();
        _viewModel.HighlightHexes(adjacentCoordinates, true);
        _viewModel.NotifyStateChanged();
    }

    private void HandleHexForDirection(Hex selectedHex)
    {
        if (_selectedHex == null) return;
        
        var adjacentCoordinates = _selectedHex.Coordinates.GetAdjacentCoordinates().ToList();
        if (!adjacentCoordinates.Contains(selectedHex.Coordinates)) return;

        _viewModel.HighlightHexes(adjacentCoordinates, false);

        var direction = _selectedHex.Coordinates.GetDirectionToNeighbour(selectedHex.Coordinates);

        _builder.SetDirection(direction);
        
        CompleteDeployment();
    }

    private void CompleteDeployment()
    {
        var command = _builder.Build();
        if (command != null && _viewModel.Game is ClientGame clientGame)
        {
            clientGame.DeployUnit(command.UnitId, new HexCoordinates(command.Position), (HexDirection)command.Direction);
        }
        
        Reset();
    }

    private void Reset()
    {
        _builder.Reset();
        _selectedHex = null;
        _currentSubState = SubState.SelectingUnit;
        _viewModel.NotifyStateChanged();
    }

    public string ActionLabel => _currentSubState switch
    {
        SubState.SelectingUnit => "Select Unit",
        SubState.SelectingHex => "Select Hex",
        SubState.SelectingDirection => "Select Direction",
        _ => string.Empty
    };

    public bool IsActionRequired => true;
}
