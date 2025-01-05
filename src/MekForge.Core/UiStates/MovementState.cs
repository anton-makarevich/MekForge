using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Core.UiStates;

public class MovementState : IUiState
{
    private readonly BattleMapViewModel _viewModel;
    private readonly MoveUnitCommandBuilder _builder;
    private Hex? _selectedHex;
    private Unit? _selectedUnit;
    
    private enum SubState
    {
        SelectingUnit,
        SelectingMovementType,
        SelectingTargetHex,
        SelectingDirection,
        Completed
    }
    
    private SubState _currentSubState = SubState.SelectingUnit;

    public MovementState(BattleMapViewModel viewModel, MoveUnitCommandBuilder builder)
    {
        _viewModel = viewModel;
        _builder = builder;
    }

    public void HandleUnitSelection(Unit? unit)
    {
        if (_currentSubState != SubState.SelectingUnit) return;
        if (unit == null) return;
        
        _selectedUnit = unit;
        _builder.SetUnit(unit);
        _currentSubState = SubState.SelectingMovementType;
        _viewModel.NotifyStateChanged();
    }

    public void HandleMovementTypeSelection(MovementType movementType)
    {
        if (_currentSubState != SubState.SelectingMovementType) return;
        
        _builder.SetMovementType(movementType);
        _currentSubState = SubState.SelectingTargetHex;
        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        switch (_currentSubState)
        {
            case SubState.SelectingUnit:
                HandleUnitSelectionFromHex(hex);
                break;
            case SubState.SelectingTargetHex:
                HandleTargetHexSelection(hex);
                break;
            case SubState.SelectingDirection:
                HandleDirectionSelection(hex);
                break;
        }
    }

    private void HandleUnitSelectionFromHex(Hex hex)
    {
        var unit = _viewModel.Units.FirstOrDefault(u => u.Position == hex.Coordinates);
        if (unit != null)
        {
            HandleUnitSelection(unit);
        }
    }

    private void HandleTargetHexSelection(Hex hex)
    {
        // TODO: Add validation for movement range and terrain restrictions
        _selectedHex = hex;
        _builder.SetDestination(hex.Coordinates);
        _currentSubState = SubState.SelectingDirection;
        
        var adjacentCoordinates = hex.Coordinates.GetAdjacentCoordinates().ToList();
        _viewModel.HighlightHexes(adjacentCoordinates, true);
        _viewModel.NotifyStateChanged();
    }

    private void HandleDirectionSelection(Hex selectedHex)
    {
        if (_selectedHex == null) return;
        
        var adjacentCoordinates = _selectedHex.Coordinates.GetAdjacentCoordinates().ToList();
        if (!adjacentCoordinates.Contains(selectedHex.Coordinates)) return;

        _viewModel.HighlightHexes(adjacentCoordinates, false);

        var direction = _selectedHex.Coordinates.GetDirectionToNeighbour(selectedHex.Coordinates);

        _builder.SetDirection(direction);
        
        CompleteMovement();
    }

    private void CompleteMovement()
    {
        var command = _builder.Build();
        if (command != null && _viewModel.Game is ClientGame clientGame)
        {
            clientGame.MoveUnit(command);
        }
        
        _builder.Reset();
        _selectedHex = null;
        _selectedUnit = null;
        _currentSubState = SubState.Completed;
        _viewModel.NotifyStateChanged();
    }

    public string ActionLabel
    {
        get
        {
            if (!IsActionRequired)
                return string.Empty;

            return _currentSubState switch
            {
                SubState.SelectingUnit => "Select unit to move",
                SubState.SelectingMovementType => "Select movement type",
                SubState.SelectingTargetHex => "Select target hex",
                SubState.SelectingDirection => "Select facing direction",
                _ => string.Empty
            };
        }
    }

    public bool IsActionRequired => _currentSubState != SubState.Completed;
}
