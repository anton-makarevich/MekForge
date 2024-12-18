using System.Reactive.Linq;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private readonly IImageService _imageService;
    private IDisposable? _gameSubscription;

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
                    NotifyPropertyChanged(nameof(Turn));
                    NotifyPropertyChanged(nameof(TurnPhase));
                    NotifyPropertyChanged(nameof(ActivePlayerName));
                });
        }
    }

    public int Turn => Game?.Turn ?? 0;

    public Phase TurnPhase => Game?.TurnPhase ?? Phase.Start;
    
    public string ActivePlayerName => Game?.ActivePlayer?.Name ?? string.Empty;

    public IImageService ImageService => _imageService;

    public void SelectHex(Hex selectedHexHex)
    {
        if (TurnPhase != Phase.Start) return;
        var player = Game?.Players[0];
        if (player != null)
        {
            (Game as ClientGame)?.SetPlayerReady(player);
        }
    }
}
