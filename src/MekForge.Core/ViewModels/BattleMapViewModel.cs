using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private BattleMap? _battleMap;
    private readonly IImageService _imageService;

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
    }

    public BattleMap? BattleMap
    {
        get => _battleMap;
        set => SetProperty(ref _battleMap, value);
    }

    public IImageService ImageService => _imageService;
}
