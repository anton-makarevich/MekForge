using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.UiStates;
using Sanet.MVVM.Core.ViewModels;
using System.Collections.ObjectModel;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Core.ViewModels.Wrappers;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private IDisposable? _gameSubscription;
    private IDisposable? _commandSubscription;
    private List<Unit> _unitsToDeploy = [];
    private Unit? _selectedUnit;
    private readonly ObservableCollection<string> _commandLog = [];
    private bool _isCommandLogExpanded;
    private bool _isRecordSheetExpanded;
    private readonly ILocalizationService _localizationService;
    private HexCoordinates _directionSelectorPosition;
    public HexCoordinates DirectionSelectorPosition
    {
        get => _directionSelectorPosition;
        private set => SetProperty(ref _directionSelectorPosition, value);
    }

    private bool _isDirectionSelectorVisible;
    public bool IsDirectionSelectorVisible
    {
        get => _isDirectionSelectorVisible;
        private set => SetProperty(ref _isDirectionSelectorVisible, value);
    }

    private IEnumerable<HexDirection>? _availableDirections;
    public IEnumerable<HexDirection>? AvailableDirections
    {
        get => _availableDirections;
        private set => SetProperty(ref _availableDirections, value);
    }

    private List<PathSegmentViewModel>? _movementPath;
    public List<PathSegmentViewModel>? MovementPath
    {
        get => _movementPath;
        private set => SetProperty(ref _movementPath, value);
    }

    public void DirectionSelectedCommand(HexDirection direction) 
    {
        CurrentState.HandleFacingSelection(direction);
    }

    public BattleMapViewModel(IImageService imageService, ILocalizationService localizationService)
    {
        ImageService = imageService;
        _localizationService = localizationService;
        CurrentState = new IdleState();
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

    public IEnumerable<WeaponSelectionViewModel> WeaponSelectionItems => 
        CurrentState is WeaponsAttackState state ? state.GetWeaponSelectionItems() : [];

    public bool IsWeaponSelectionVisible => 
        CurrentState is WeaponsAttackState
        {
            CurrentStep: WeaponsAttackStep.TargetSelection, SelectedTarget: not null
        };

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
        
        _gameSubscription = localGame.TurnChanges
            .StartWith(localGame.Turn)
            .CombineLatest<int, PhaseNames, IPlayer?, int, (int Turn, PhaseNames Phase, IPlayer? Player, int UnitsToPlay)>(localGame.PhaseChanges.StartWith(localGame.TurnPhase),
                localGame.ActivePlayerChanges.StartWith(localGame.ActivePlayer),
                localGame.UnitsToPlayChanges.StartWith(localGame.UnitsToPlayCurrentStep),
                (turn, phase, player, units) => (turn, phase, player, units))
            .Subscribe(_ =>
            {
                CleanSelection();
                UpdateGamePhase();
                NotifyStateChanged();
            });
    }

    private void UpdateGamePhase()
    {
        if (Game is not ClientGame { ActivePlayer: not null } clientGame)
        {
            TransitionToState(new IdleState());
            return;
        }

        switch (TurnPhaseNames)
        {
            case PhaseNames.Deployment when clientGame.ActivePlayer.Units.Any(u => !u.IsDeployed):
                TransitionToState(new DeploymentState(this));
                ShowUnitsToDeploy();
                break;
            
            case PhaseNames.Movement when clientGame.UnitsToPlayCurrentStep > 0:
                TransitionToState(new MovementState(this));
                break;
            
            case PhaseNames.WeaponsAttack when clientGame.UnitsToPlayCurrentStep > 0:
                TransitionToState(new WeaponsAttackState(this));
                break;
            
            default:
                TransitionToState(new IdleState());
                break;
        }
    }

    private void ShowUnitsToDeploy()
    {
        if (Game?.ActivePlayer == null || Game?.UnitsToPlayCurrentStep < 1)
        {
            UnitsToDeploy = [];
            return;
        }
        UnitsToDeploy = Game?.ActivePlayer?.Units.Where(u => !u.IsDeployed).ToList()??[];
    }

    private void TransitionToState(IUiState newState)
    {
        CurrentState = newState;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        NotifyPropertyChanged(nameof(Turn));
        NotifyPropertyChanged(nameof(TurnPhaseNames));
        NotifyPropertyChanged(nameof(ActivePlayerName));
        NotifyPropertyChanged(nameof(ActivePlayerTint));
        NotifyPropertyChanged(nameof(UserActionLabel));
        NotifyPropertyChanged(nameof(IsUserActionLabelVisible));
        NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
    }

    internal void HighlightHexes(List<HexCoordinates> coordinates, bool isHighlighted)
    {
        var hexesToHighlight = Game?.BattleMap.GetHexes().Where(h => coordinates.Contains(h.Coordinates)).ToList();
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

    public bool AreUnitsToDeployVisible => CurrentState is DeploymentState
                                          && UnitsToDeploy.Count > 0
                                          && SelectedUnit == null;

    public int Turn => Game?.Turn ?? 0;

    public PhaseNames TurnPhaseNames => Game?.TurnPhase ?? PhaseNames.Start;
    
    public string ActivePlayerName => Game?.ActivePlayer?.Name ?? string.Empty;

    public string ActivePlayerTint => Game?.ActivePlayer?.Tint ?? "#FFFFFF";

    public IImageService ImageService { get; }

    public Unit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (value == _selectedUnit) return;
            SetProperty(ref _selectedUnit, value);
            CurrentState.HandleUnitSelection(value);
            NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
            NotifyPropertyChanged(nameof(IsRecordSheetButtonVisible));
            NotifyPropertyChanged(nameof(IsRecordSheetPanelVisible));
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

        CurrentState.HandleHexSelection(selectedHex);
    }

    private void CleanSelection()
    {
        SelectedUnit = null;
    }

    public string UserActionLabel => CurrentState.ActionLabel;
    public bool IsUserActionLabelVisible => CurrentState.IsActionRequired;

    public bool IsCommandLogExpanded
    {
        get => _isCommandLogExpanded;
        set => SetProperty(ref _isCommandLogExpanded, value);
    }

    public bool IsRecordSheetExpanded
    {
        get => _isRecordSheetExpanded;
        set
        {
            SetProperty(ref _isRecordSheetExpanded, value); 
            NotifyPropertyChanged(nameof(IsRecordSheetButtonVisible));
            NotifyPropertyChanged(nameof(IsRecordSheetPanelVisible));
        }
    }

    public bool IsRecordSheetButtonVisible => SelectedUnit != null && !IsRecordSheetExpanded;
    public bool IsRecordSheetPanelVisible => SelectedUnit != null && IsRecordSheetExpanded;

    public void ToggleCommandLog()
    {
        IsCommandLogExpanded = !IsCommandLogExpanded;
    }

    public void ToggleRecordSheet()
    {
        IsRecordSheetExpanded = !IsRecordSheetExpanded;
    }

    public IEnumerable<Unit> Units => Game?.Players.SelectMany(p => p.Units) ?? [];
    public IUiState CurrentState { get; private set; }

    public void ShowDirectionSelector(HexCoordinates position, IEnumerable<HexDirection> availableDirections)
    {
        DirectionSelectorPosition = position;
        AvailableDirections = availableDirections;
        IsDirectionSelectorVisible = true;
    }

    public void HideDirectionSelector()
    {
        IsDirectionSelectorVisible = false;
        AvailableDirections = null;
    }

    public void ShowMovementPath(List<PathSegment> path)
    {
        HideMovementPath();
        if (path.Count < 1)
        {
            return;
        }

        var segments = path.Select(p=> new PathSegmentViewModel(p)).ToList();
        MovementPath = segments;
    }

    public void HideMovementPath()
    {
        MovementPath = null;
    }
}
