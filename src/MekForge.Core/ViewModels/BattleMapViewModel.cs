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
        if (_game != null)
        {
            _gameSubscription = Observable
                .Interval(TimeSpan.FromMilliseconds(100)) // Adjust the interval as needed
                .Select(_ => new
                {
                    _game.Turn,
                    _game.TurnPhase,
                    _game.ActivePlayer
                })
                .DistinctUntilChanged()
                .Subscribe(_ =>
                {
                    UpdateGameState();
                    CheckPlayerActionState();
                });
        }
    }

    private void UpdateGameState()
    {
        NotifyPropertyChanged(nameof(Turn));
        NotifyPropertyChanged(nameof(TurnPhase));
        NotifyPropertyChanged(nameof(ActivePlayerName));
    }

    private void CheckPlayerActionState()
    {
        if (Game is not ClientGame clientGame) return;              // No game
       
        AwaitedAction = clientGame.GetNextClientAction(AwaitedAction); 
        
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
        }
    }

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
            CheckPlayerActionState();
        }
    }
    
    private Hex? _selectedHex=null;
    

    public void SelectHex(Hex selectedHex)
    {
        if (TurnPhase == Phase.Start)
        {
            var player = Game?.Players[0];
            if (player != null)
            {
                (Game as ClientGame)?.SetPlayerReady(player);
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
                }
                else
                {
                    var adjustedHex = _selectedHex.Coordinates.GetAdjacentCoordinates().ToList();
                    if (Game is ClientGame localGame)
                    {
                        HighlightHexes(adjustedHex,false);
                    }

                    _selectedHex = null;
                }
            }
        }
    }

    private void HighlightHexes(List<HexCoordinates> adjustedHex, bool isHighlighted)
    {
        var hexesToHighlight = _game?.GetHexes().Where(h => adjustedHex.Contains(h.Coordinates)).ToList();
        foreach (var hex in hexesToHighlight)
        {
            hex.IsHighlighted = isHighlighted;
        }
    }
}
