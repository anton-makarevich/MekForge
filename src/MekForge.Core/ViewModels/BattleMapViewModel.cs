using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private BattleState? _battleState;
    private readonly IImageService _imageService;

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
    }

    public BattleState? BattleState
    {
        get => _battleState;
        set => SetProperty(ref _battleState, value);
    }

    public IImageService ImageService => _imageService;

    public void SelectHex(Hex selectedHexHex)
    {
        //pass action to battlestate, battlestate should decide what to do with it based on the state
    }
}
