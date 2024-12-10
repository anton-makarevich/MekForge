using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private BattleMap? _battleMap;
    private readonly IImageService _imageService;
    private static Unit? _unit;

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

    public Unit? Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }
}
