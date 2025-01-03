using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.UiStates;
using Sanet.MVVM.Core.ViewModels;
using System.Collections.ObjectModel;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private IDisposable? _gameSubscription;
    private IDisposable? _commandSubscription;
    private IUiState _currentState;
    private DeploymentCommandBuilder? _deploymentBuilder;
    private List<Unit> _unitsToDeploy = [];
    private Unit? _selectedUnit;
    private readonly ObservableCollection<string> _commandLog = [];
    private bool _isCommandLogExpanded;
    private readonly ILocalizationService _localizationService;

    public BattleMapViewModel(IImageService imageService, ILocalizationService localizationService)
    {
        ImageService = imageService;
        _localizationService = localizationService;
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

    public IReadOnlyCollection<string> CommandLog => _commandLog;

    private void SubscribeToGameChanges()
    {
        _gameSubscription?.Dispose();
        _commandSubscription?.Dispose();
        
        if (Game is not ClientGame localGame) return;

        _commandSubscription = localGame.Commands
            .Subscribe(command =>
            {
                var formattedCommand = command.Format(_localizationService, Game);
                _commandLog.Add(formattedCommand);
                NotifyPropertyChanged(nameof(CommandLog));
            });
        
        _gameSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(100))
            .Select(_ => new
            {
                localGame.Turn,
                localGame.TurnPhase,
                localGame.ActivePlayer,
                localGame.UnitsToMoveCurrentStep
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
        if (TurnPhaseNames == PhaseNames.Deployment 
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
        if (Game?.ActivePlayer == null || Game?.UnitsToMoveCurrentStep < 1)
        {
            UnitsToDeploy = [];
            return;
        }
        UnitsToDeploy = Game?.ActivePlayer?.Units.Where(u => !u.IsDeployed).ToList()??[];
    }

    private void TransitionToState(IUiState newState)
    {
        _currentState = newState;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        NotifyPropertyChanged(nameof(Turn));
        NotifyPropertyChanged(nameof(TurnPhaseNames));
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

    public PhaseNames TurnPhaseNames => Game?.TurnPhase ?? PhaseNames.Start;
    
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
        if (TurnPhaseNames == PhaseNames.Start)
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

    public bool IsCommandLogExpanded
    {
        get => _isCommandLogExpanded;
        set => SetProperty(ref _isCommandLogExpanded, value);
    }

    public void ToggleCommandLog()
    {
        IsCommandLogExpanded = !IsCommandLogExpanded;
    }

    public IEnumerable<Unit> Units => Game?.Players.SelectMany(p => p.Units) ?? [];
}
