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
    private List<HexCoordinates> _reachableHexes = [];

    public MovementState(BattleMapViewModel viewModel)
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
        _builder = new MoveUnitCommandBuilder(_viewModel.Game.Id, _viewModel.Game.ActivePlayer.Id);
    }

    public void HandleUnitSelection(Unit? unit)
    {
        if (CurrentMovementStep != MovementStep.SelectingUnit) return;
        if (unit == null) return;
        
        _selectedUnit = unit;
        _builder.SetUnit(unit);
        CurrentMovementStep = MovementStep.SelectingMovementType;
        _viewModel.NotifyStateChanged();
    }

    public void HandleMovementTypeSelection(MovementType movementType)
    {
        if (CurrentMovementStep != MovementStep.SelectingMovementType) return;
        
        _builder.SetMovementType(movementType);
        CurrentMovementStep = MovementStep.SelectingTargetHex;
        var mp = _selectedUnit?.GetMovementPoints(movementType) ?? 0;

        // Get reachable hexes and highlight them
        if (_selectedUnit?.Position != null && _viewModel.Game != null)
        {
            var prohibitedHexes = _viewModel.Units
                .Where(u=>u.Owner?.Id != _viewModel.Game.ActivePlayer?.Id)
                .Where(u=> u.Position != null)
                .Select(u => u.Position.Value.Coordinates)
                .ToList();
            _reachableHexes = _viewModel.Game.BattleMap.GetReachableHexes(_selectedUnit.Position.Value, mp, prohibitedHexes)
                .Select(x=>x.coordinates)
                .ToList();
            _viewModel.HighlightHexes(_reachableHexes, true);
        }

        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        switch (CurrentMovementStep)
        {
            case MovementStep.SelectingUnit:
                HandleUnitSelectionFromHex(hex);
                break;
            case MovementStep.SelectingTargetHex:
                HandleTargetHexSelection(hex);
                break;
            case MovementStep.SelectingDirection:
                HandleDirectionSelection(hex);
                break;
        }
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        throw new NotImplementedException();
    }

    private void HandleUnitSelectionFromHex(Hex hex)
    {
        var unit = _viewModel.Units.FirstOrDefault(u => u.Position?.Coordinates == hex.Coordinates);
        if (unit != null && unit.Owner?.Id==_viewModel.Game?.ActivePlayer?.Id)
        {
            _viewModel.SelectedUnit=unit;
        }
    }

    private void HandleTargetHexSelection(Hex hex)
    {
        // TODO: Add validation for movement range and terrain restrictions
        _selectedHex = hex;
        _builder.SetDestination(hex.Coordinates);
        CurrentMovementStep = MovementStep.SelectingDirection;
        
        // Clear movement range highlight and highlight adjacent hexes for direction selection
        if (_selectedUnit != null && _viewModel.Game != null)
        {
            _viewModel.HighlightHexes(_reachableHexes, false);
        }
        
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
        CurrentMovementStep = MovementStep.Completed;
        _viewModel.NotifyStateChanged();
    }

    public string ActionLabel
    {
        get
        {
            if (!IsActionRequired)
                return string.Empty;

            return CurrentMovementStep switch
            {
                MovementStep.SelectingUnit => "Select unit to move",
                MovementStep.SelectingMovementType => "Select movement type",
                MovementStep.SelectingTargetHex => "Select target hex",
                MovementStep.SelectingDirection => "Select facing direction",
                _ => string.Empty
            };
        }
    }

    public bool IsActionRequired => CurrentMovementStep != MovementStep.Completed;
    
    public MovementStep CurrentMovementStep { get; private set; } = MovementStep.SelectingUnit;
}
