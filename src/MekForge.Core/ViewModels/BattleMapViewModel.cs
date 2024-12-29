using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private readonly IImageService _imageService;
    private IDisposable? _gameSubscription;
    private PlayerActions _awaitedAction = PlayerActions.None;
    private List<Unit> _unitsToDeploy = [];
    private Unit? _selectedUnit = null;
    private HexDirection? _selectedDirection = null;
    

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
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
        _gameSubscription?.Dispose(); // Dispose of previous subscription
        if (Game is not ClientGame localGame) return;
        _gameSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(100)) // Adjust the interval as needed
            .Select(_ => new
            {
                localGame.Turn,
                localGame.TurnPhase,
                localGame.ActivePlayer
            })
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                CleanSelection();
                UpdateGameState();
                AwaitedAction = GetNextClientAction(); 
            });
    }

    private void UpdateGameState()
    {
        NotifyPropertyChanged(nameof(Turn));
        NotifyPropertyChanged(nameof(TurnPhase));
        NotifyPropertyChanged(nameof(ActivePlayerName));
    } 

    private PlayerActions AwaitedAction
    {
        get => _awaitedAction;
        set
        {
            SetProperty(ref _awaitedAction, value);
            if (value == PlayerActions.SelectUnitToDeploy)
            {
                ShowUnitsToDeploy();
            }

            if (value == PlayerActions.SelectHex)
            {
                _selectedHex = null;
            }
            NotifyPropertyChanged(nameof(UserActionLabel));
            NotifyPropertyChanged(nameof(IsUserActionLabelVisible));
        }
    }
    
    private PlayerActions GetNextClientAction()
    {
        if (!ActionPossible) return PlayerActions.None;
        if (TurnPhase == Phase.Deployment)
        {
            if (AwaitedAction == PlayerActions.SelectUnitToDeploy)
            {
                return PlayerActions.SelectHex;
            }
            if (AwaitedAction == PlayerActions.SelectHex)
            {
                return PlayerActions.SelectDirection;
            }
            var hasUnitsToDeploy = Game?.ActivePlayer?.Units.Any(u => !u.IsDeployed);
            if (hasUnitsToDeploy == true) return PlayerActions.SelectUnitToDeploy;
        }
        return PlayerActions.None;
    }

    private bool ActionPossible => Game?.ActivePlayer != null
                                             && ((ClientGame)Game).LocalPlayers.Any(lp => lp.Id == Game.ActivePlayer.Id);

    private void ShowUnitsToDeploy()
    {
        if (Game?.ActivePlayer == null) return;
        UnitsToDeploy = Game.ActivePlayer.Units.Where(u => !u.IsDeployed).ToList();
    }

    public List<Unit> UnitsToDeploy
    {
        get => _unitsToDeploy;
        private set
        {
            SetProperty(ref _unitsToDeploy,value);
            NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
        }
    }

    public bool AreUnitsToDeployVisible => AwaitedAction == PlayerActions.SelectUnitToDeploy
                                            && UnitsToDeploy.Count > 0
                                            && SelectedUnit == null;

    public int Turn => Game?.Turn ?? 0;

    public Phase TurnPhase => Game?.TurnPhase ?? Phase.Start;
    
    public string ActivePlayerName => Game?.ActivePlayer?.Name ?? string.Empty;

    public IImageService ImageService => _imageService;

    public Unit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            SetProperty(ref _selectedUnit, value);
            NotifyPropertyChanged(nameof(AreUnitsToDeployVisible));
            AwaitedAction = GetNextClientAction(); 
        }
    }
    
    private Hex? _selectedHex=null;

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
        }

        if (TurnPhase == Phase.Deployment)
        {
            if (AwaitedAction == PlayerActions.SelectHex)
            {
                if (_selectedHex == null)
                {
                    _selectedHex = selectedHex;
                    var adjustedHex = _selectedHex.Coordinates.GetAdjacentCoordinates().ToList();
                    if (Game is ClientGame localGame)
                    {
                        HighlightHexes(adjustedHex,true);
                    }
                    AwaitedAction = GetNextClientAction(); 
                    return;
                }
            }

            if (AwaitedAction == PlayerActions.SelectDirection)
            {
                if (_selectedHex == null) return;
                var adjustedHex = _selectedHex.Coordinates.GetAdjacentCoordinates().ToList();
                if (Game is not ClientGame localGame) return;
                if (!adjustedHex.Contains(selectedHex.Coordinates)) return;
                HighlightHexes(adjustedHex, false);

                _selectedDirection = _selectedHex.Coordinates.GetDirectionToNeighbour(selectedHex.Coordinates);
                if (_selectedDirection == null) return;

                if (SelectedUnit != null)
                {
                    localGame.DeployUnit(SelectedUnit.Id,
                        _selectedHex.Coordinates,
                        _selectedDirection.Value);
                }
            }
        }
    }

    private void CleanSelection()
    {
        
        SelectedUnit = null;
    }

    private void HighlightHexes(List<HexCoordinates> adjustedHex, bool isHighlighted)
    {
        var hexesToHighlight = _game?.GetHexes().Where(h => adjustedHex.Contains(h.Coordinates)).ToList();
        if (hexesToHighlight == null) return;
        foreach (var hex in hexesToHighlight)
        {
            hex.IsHighlighted = isHighlighted;
        }
    }

    public string UserActionLabel
    {
        get
        {
            return AwaitedAction switch
            {
                PlayerActions.None => "",
                PlayerActions.SelectUnitToDeploy => "Select Unit",
                PlayerActions.SelectHex => "Select hex",
                PlayerActions.SelectDirection => "Select facing direction",
                _ => ""
            };
        }
    }

    public bool IsUserActionLabelVisible => !string.IsNullOrEmpty(UserActionLabel);
    public IEnumerable<Unit> Units => (Game==null)
        ? new List<Unit>()
        : Game.Players.Select(u=>u.Units).SelectMany(u=>u);
}
