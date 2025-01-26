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
    private Unit? _selectedUnit;
    private List<HexCoordinates> _forwardReachableHexes = [];
    private List<HexCoordinates> _backwardReachableHexes = [];
    private readonly List<HexCoordinates> _prohibitedHexes;
    private MovementType? _selectedMovementType;
    private int _movementPoints;
    private Dictionary<HexDirection, List<PathSegment>> _possibleDirections;

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
        if (unit == null) return;
        if (unit.HasMoved) return;
        
        _selectedUnit = unit;
        _builder.SetUnit(unit);
        CurrentMovementStep = MovementStep.SelectingMovementType;
        _viewModel.NotifyStateChanged();
    }

    public void HandleMovementTypeSelection(MovementType movementType)
    {
        if (_selectedUnit == null) return;
        if (CurrentMovementStep != MovementStep.SelectingMovementType) return;
        _selectedMovementType = movementType;
        _builder.SetMovementType(movementType);
        
        if (movementType == MovementType.StandingStill)
        {
            // For standing still, we create a single path segment with same From and To positions
            var path = new List<PathSegment>();
            _builder.SetMovementPath(path);
            CompleteMovement();
            return;
        }

        CurrentMovementStep = MovementStep.SelectingTargetHex;
        _movementPoints = _selectedUnit?.GetMovementPoints(movementType) ?? 0;

        // Get reachable hexes and highlight them
        if (_selectedUnit?.Position != null && _viewModel.Game != null)
        {
            if (movementType == MovementType.Jump)
            {
                // For jumping, we use the simplified method that ignores terrain and facing
                var reachableHexes = _viewModel.Game.BattleMap
                    .GetJumpReachableHexes(
                        _selectedUnit.Position.Value.Coordinates,
                        _movementPoints,
                        _prohibitedHexes)
                    .Where(hex => !_viewModel.Units
                        .Any(u => u.Owner?.Id == _viewModel.Game.ActivePlayer?.Id && u.Position?.Coordinates == hex))
                    .ToList();
                
                // For jumping, there's no forward/backward distinction
                _forwardReachableHexes = reachableHexes;
                _backwardReachableHexes = [];
            }
            else
            {
                // Get forward reachable hexes
                _forwardReachableHexes = _viewModel.Game.BattleMap
                    .GetReachableHexes(_selectedUnit.Position.Value, _movementPoints, _prohibitedHexes)
                    .Select(x => x.coordinates)
                    .Where(hex => !_viewModel.Units
                        .Any(u => u.Owner?.Id == _viewModel.Game.ActivePlayer?.Id && u.Position?.Coordinates == hex))
                    .ToList();

                // Get backward reachable hexes if unit can move backward
                _backwardReachableHexes = [];
                if (_selectedUnit.CanMoveBackward(movementType))
                {
                    var oppositePosition = _selectedUnit.Position.Value.GetOppositeDirectionPosition();
                    _backwardReachableHexes = _viewModel.Game.BattleMap
                        .GetReachableHexes(oppositePosition, _movementPoints, _prohibitedHexes)
                        .Select(x => x.coordinates)
                        .Where(hex => !_viewModel.Units
                            .Any(u => u.Owner?.Id == _viewModel.Game.ActivePlayer?.Id && u.Position?.Coordinates == hex))
                        .ToList();
                }
            }

            // Highlight all reachable hexes
            var allReachableHexes = _forwardReachableHexes.Union(_backwardReachableHexes).ToList();
            _viewModel.HighlightHexes(allReachableHexes, true);
        }

        _viewModel.NotifyStateChanged();
    }

    public void HandleHexSelection(Hex hex)
    {
        if (HandleUnitSelectionFromHex(hex)) return;
        HandleTargetHexSelection(hex);
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        if (CurrentMovementStep != MovementStep.SelectingDirection) return;
        var path = _possibleDirections[direction]; 
        _builder.SetMovementPath(path);
        if (_selectedMovementType == MovementType.Jump || (_viewModel.MovementPath != null && _viewModel.MovementPath.Last().To==path.Last().To))
        {
            CompleteMovement();
            return;
        }
        _viewModel.ShowMovementPath(path);
    }

    private bool HandleUnitSelectionFromHex(Hex hex)
    {
        var unit = _viewModel.Units.FirstOrDefault(u => u.Position?.Coordinates == hex.Coordinates);
        if (unit == null || unit.Owner?.Id != _viewModel.Game?.ActivePlayer?.Id) return false;
        ResetUnitSelection();
        _viewModel.SelectedUnit=unit;
        return true;
    }

    private void ResetUnitSelection()
    {
        if (_viewModel.SelectedUnit == null) return;
        _viewModel.SelectedUnit = null;
        _viewModel.HideMovementPath();
        _viewModel.HideDirectionSelector();
        if (_forwardReachableHexes.Count > 0 || _backwardReachableHexes.Count > 0)
        {
            var allReachableHexes = _forwardReachableHexes.Union(_backwardReachableHexes).ToList();
            _viewModel.HighlightHexes(allReachableHexes, false);
            _forwardReachableHexes = [];
            _backwardReachableHexes = [];
        }
        CurrentMovementStep=MovementStep.SelectingUnit;
        _viewModel.NotifyStateChanged();
    }

    private void HandleTargetHexSelection(Hex hex)
    {
        if (_selectedUnit?.Position == null || _viewModel.Game == null) return;

        var isForwardReachable = _forwardReachableHexes.Contains(hex.Coordinates);
        var isBackwardReachable = _backwardReachableHexes.Contains(hex.Coordinates);
        
        if (!isForwardReachable && !isBackwardReachable) return;

        CurrentMovementStep = MovementStep.SelectingDirection;
        
        _possibleDirections = new Dictionary<HexDirection, List<PathSegment>>();
        var availableDirections = Enum.GetValues<HexDirection>();

        if (_selectedMovementType == MovementType.Jump)
        {
            // For jumping, we can face any direction and move directly to target
            foreach (var direction in availableDirections)
            {
                var path = new List<PathSegment>
                {
                    new(
                        _selectedUnit.Position.Value,
                        new HexPosition(hex.Coordinates, direction),
                        1) // Cost is always 1 for jumping
                };
                _possibleDirections[direction] = path;
            }
        }
        else
        {
            foreach (var direction in availableDirections)
            {
                var targetPos = new HexPosition(hex.Coordinates, direction);
                List<PathSegment>? path = null;

                if (isForwardReachable)
                {
                    // Try forward movement
                    path = _viewModel.Game?.BattleMap.FindPath(
                        _selectedUnit.Position.Value,
                        targetPos,
                        _movementPoints,
                        _prohibitedHexes);
                }

                if (path == null && isBackwardReachable)
                {
                    // Try backward movement
                    var oppositeStartPos = _selectedUnit.Position.Value.GetOppositeDirectionPosition();
                    var oppositeTargetPos = targetPos.GetOppositeDirectionPosition();
                    
                    path = _viewModel.Game?.BattleMap.FindPath(
                        oppositeStartPos,
                        oppositeTargetPos,
                        _movementPoints,
                        _prohibitedHexes);

                    // If path found, swap all directions in path segments
                    path = path?.Select(segment => new PathSegment(
                        new HexPosition(segment.From.Coordinates, segment.From.Facing.GetOppositeDirection()),
                        new HexPosition(segment.To.Coordinates, segment.To.Facing.GetOppositeDirection()),
                        segment.Cost
                    )).ToList();
                }

                if (path != null)
                {
                    _possibleDirections[direction] = path;
                }
            }
        }

        // Show direction selector if there are any possible directions
        if (_possibleDirections.Count != 0)
        {
            _viewModel.HideMovementPath();
            _viewModel.ShowDirectionSelector(hex.Coordinates, _possibleDirections.Select(kv=>kv.Key).ToList());
        }

        _viewModel.NotifyStateChanged();
    }
    
    private void CompleteMovement()
    {
        var command = _builder.Build();
        if (command != null && _viewModel.Game is ClientGame clientGame)
        {
            _viewModel.HideMovementPath();
            _viewModel.HideDirectionSelector();
            clientGame.MoveUnit(command);
        }
        
        _builder.Reset();
        var allReachableHexes = _forwardReachableHexes.Union(_backwardReachableHexes).ToList();
        _viewModel.HighlightHexes(allReachableHexes,false);
        _forwardReachableHexes = [];
        _backwardReachableHexes = [];
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
