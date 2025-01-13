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
    private Hex? _targetHex;
    private Unit? _selectedUnit;
    private List<HexCoordinates> _reachableHexes = [];
    private readonly List<HexCoordinates> _prohibitedHexes;
    private int _movementPoints;

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
        
        // Get hexes with enemy units - these will be excluded from pathfinding
        _prohibitedHexes = _viewModel.Units
            .Where(u=>u.Owner?.Id != _viewModel.Game.ActivePlayer?.Id && u.Position.HasValue)
            .Select(u => u.Position.Value.Coordinates)
            .ToList();
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
        
        _movementPoints = _selectedUnit?.GetMovementPoints(movementType) ?? 0;

        // Get reachable hexes and highlight them
        if (_selectedUnit?.Position != null && _viewModel.Game != null)
        {
            // Get all reachable hexes, excluding enemy positions from pathfinding
            _reachableHexes = _viewModel.Game.BattleMap.GetReachableHexes(_selectedUnit.Position.Value, _movementPoints, _prohibitedHexes)
                .Select(x=>x.coordinates)
                .Where(hex => !_viewModel.Units
                    .Any(u => u.Owner?.Id == _viewModel.Game.ActivePlayer?.Id && u.Position?.Coordinates == hex))
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
            case MovementStep.SelectingDirection:
                HandleTargetHexSelection(hex);
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
        // Check if the hex is actually reachable
        if (_reachableHexes.All(h => h != hex.Coordinates))
        {
            return;
        }

        _targetHex = hex;
        _builder.SetDestination(hex.Coordinates);
        CurrentMovementStep = MovementStep.SelectingDirection;
        
        if (_selectedUnit != null && _viewModel.Game != null && _selectedUnit.Position !=null)
        {
            // Find all possible facing directions
            var possibleDirections = new List<HexDirection>();
            var startPosition = new HexPosition(_selectedUnit.Position.Value.Coordinates, _selectedUnit.Position.Value.Facing);
            
            foreach (var direction in Enum.GetValues<HexDirection>())
            {
                var targetPosition = new HexPosition(hex.Coordinates, direction);
                var path = _viewModel.Game.BattleMap.FindPath(startPosition, targetPosition, _movementPoints, _prohibitedHexes);
                
                if (path != null)
                {
                    possibleDirections.Add(direction);
                }
            }
            
            // Show direction selector if there are any possible directions
            if (possibleDirections.Count != 0)
            {
                _viewModel.ShowDirectionSelector(hex.Coordinates, possibleDirections);
            }
        }
        
        _viewModel.NotifyStateChanged();
    }

    private void HandleDirectionSelection(Hex selectedHex)
    {
        if (_targetHex == null) return;
        
        var adjacentCoordinates = _targetHex.Coordinates.GetAdjacentCoordinates().ToList();
        if (!adjacentCoordinates.Contains(selectedHex.Coordinates)) return;

        _viewModel.HighlightHexes(adjacentCoordinates, false);

        var direction = _targetHex.Coordinates.GetDirectionToNeighbour(selectedHex.Coordinates);

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
        _targetHex = null;
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
