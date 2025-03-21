using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.ViewModels;

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
        SelectingDirection,
        Completed
    }
    
    private SubState _currentSubState = SubState.SelectingUnit;

    public DeploymentState(BattleMapViewModel viewModel)
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
        _builder = new DeploymentCommandBuilder(_viewModel.Game.Id, _viewModel.Game.ActivePlayer.Id);
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
        if (_currentSubState is SubState.SelectingHex 
                             or SubState.SelectingDirection) HandleHexForDeployment(hex);
    }

    public void HandleFacingSelection(HexDirection direction)
    {
        if (_currentSubState != SubState.SelectingDirection) return;
        _builder.SetDirection(direction);
        _viewModel.HideDirectionSelector();
        CompleteDeployment();
    }

    private static IEnumerable<HexDirection> GetAllDirections()
    {
        return
        [
            HexDirection.Top,
            HexDirection.TopRight,
            HexDirection.BottomRight,
            HexDirection.Bottom,
            HexDirection.BottomLeft,
            HexDirection.TopLeft
        ];
    }

    private void HandleHexForDeployment(Hex hex)
    {
        if (IsHexOccupied(hex))
            return;
            
        _selectedHex = hex;
        _builder.SetPosition(hex.Coordinates);
        _currentSubState = SubState.SelectingDirection;
        
        _viewModel.ShowDirectionSelector(_selectedHex.Coordinates, GetAllDirections());
    }

    private bool IsHexOccupied(Hex hex)
    {
        return _viewModel.Units.Any(u => u.IsDeployed &&u.Position!.Coordinates == hex.Coordinates);
    }

    private void CompleteDeployment()
    {
        var command = _builder.Build();
        if (command != null && _viewModel.Game is ClientGame clientGame)
        {
            clientGame.DeployUnit(command.Value);
        }
        
        _builder.Reset();
        _selectedHex = null;
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
                SubState.SelectingUnit => "Select Unit",
                SubState.SelectingHex => "Select Hex",
                SubState.SelectingDirection => "Select Direction",
                _ => string.Empty
            };
        }
    }

    public bool IsActionRequired
    {
        get
        {
            if (_viewModel.Game is not ClientGame clientGame)
                return false;
            return clientGame is { ActivePlayer: not null, UnitsToPlayCurrentStep: > 0 }
                   && clientGame.LocalPlayers.FirstOrDefault(p=>p.Id==clientGame.ActivePlayer.Id)!=null;
        }
    }
}
