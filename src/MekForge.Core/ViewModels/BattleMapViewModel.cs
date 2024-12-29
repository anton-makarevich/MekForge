using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.UiStates;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private IDisposable? _gameSubscription;
    private IUiState _currentState;
    private DeploymentCommandBuilder? _deploymentBuilder;
    private List<Unit> _unitsToDeploy = [];
    private Unit? _selectedUnit;

    public BattleMapViewModel(IImageService imageService)
    {
        ImageService = imageService;
        _currentState = new IdleState();
    }

    public IGame? Game
    {
        get => _game;
        set
        {
            SetProperty(ref _game, value);
            SubscribeToGameChanges();
        }
    }

    private void SubscribeToGameChanges()
    {
        _gameSubscription?.Dispose();
        if (Game is not ClientGame localGame) return;
        _gameSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(100))
            .Select(_ => new
            {
                localGame.Turn,
                localGame.TurnPhase,
                localGame.ActivePlayer,
                UndeployedUnits = localGame.ActivePlayer?.Units.Count(u => !u.IsDeployed) ?? 0
            })
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                CleanSelection();
                UpdateGamePhase();
                NotifyStateChanged();
            });
    }

    private void UpdateGamePhase()
    {
        if (TurnPhase == Phase.Deployment 
            && Game is ClientGame { ActivePlayer: not null } clientGame
            && clientGame.ActivePlayer?.Units.Any(u => !u.IsDeployed) == true)
        {
            _deploymentBuilder = new DeploymentCommandBuilder(clientGame.GameId,
                    clientGame.ActivePlayer.Id);
            
            TransitionToState(new DeploymentState(this, _deploymentBuilder));
            
            ShowUnitsToDeploy();
        }
        else
        {
            TransitionToState(new IdleState());
        }
    }

    private void ShowUnitsToDeploy()
    {
        if (Game?.ActivePlayer == null) return;
        UnitsToDeploy = Game.ActivePlayer.Units.Where(u => !u.IsDeployed).ToList();
    }

    private void TransitionToState(IUiState newState)
    {
        _currentState = newState;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        NotifyPropertyChanged(nameof(Turn));
        NotifyPropertyChanged(nameof(TurnPhase));
        NotifyPropertyChanged(nameof(ActivePlayerName));
        NotifyPropertyChanged(nameof(UserActionLabel));
        NotifyPropertyChanged(nameof(IsUserActionLabelVisible));
        NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
    }

    internal void HighlightHexes(List<HexCoordinates> coordinates, bool isHighlighted)
    {
        var hexesToHighlight = Game?.GetHexes().Where(h => coordinates.Contains(h.Coordinates)).ToList();
        if (hexesToHighlight == null) return;
        foreach (var hex in hexesToHighlight)
        {
            hex.IsHighlighted = isHighlighted;
        }
    }

    public List<Unit> UnitsToDeploy
    {
        get => _unitsToDeploy;
        private set
        {
            SetProperty(ref _unitsToDeploy, value);
            NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
        }
    }

    public bool AreUnitsToDeployVisible => _currentState is DeploymentState
                                          && UnitsToDeploy.Count > 0
                                          && SelectedUnit == null;

    public int Turn => Game?.Turn ?? 0;

    public Phase TurnPhase => Game?.TurnPhase ?? Phase.Start;
    
    public string ActivePlayerName => Game?.ActivePlayer?.Name ?? string.Empty;

    public IImageService ImageService { get; }

    public Unit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (value == _selectedUnit) return;
            SetProperty(ref _selectedUnit, value);
            _currentState.HandleUnitSelection(value);
            NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
        }
    }

    public void HandleHexSelection(Hex selectedHex)
    {
        if (TurnPhase == Phase.Start)
        {
            if (Game is ClientGame localGame)
            {
                foreach (var player in localGame.Players)
                {
                    localGame.SetPlayerReady(player);
                }
            }
            return;
        }

        _currentState.HandleHexSelection(selectedHex);
    }

    private void CleanSelection()
    {
        SelectedUnit = null;
    }

    public string UserActionLabel => _currentState.ActionLabel;
    public bool IsUserActionLabelVisible => _currentState.IsActionRequired;

    public IEnumerable<Unit> Units => Game?.Players.SelectMany(p => p.Units) ?? [];
}
