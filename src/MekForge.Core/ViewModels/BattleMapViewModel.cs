using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private IGame? _game;
    private readonly IImageService _imageService;

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
    }

    public IGame? Game
    {
        get => _game;
        set => SetProperty(ref _game, value);
    }
    
    public int Turn => Game?.Turn ?? 0;

    public Phase TurnPhase => Game?.TurnPhase ?? Phase.Start;
    
    public string ActivePlayerName => Game?.ActivePlayer?.Name ?? string.Empty;

    public IImageService ImageService => _imageService;

    public void SelectHex(Hex selectedHexHex)
    {
        //pass action to battlestate, battlestate should decide what to do with it based on the state
    }
}
