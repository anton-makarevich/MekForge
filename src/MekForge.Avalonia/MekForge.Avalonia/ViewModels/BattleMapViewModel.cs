using Sanet.MekForge.Avalonia.Services;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private readonly BattleMap _battleMap;
    private readonly IImageService _imageService;

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
        const int width = 17;
        const int height = 16;
        
        // Generate a map with clear terrain and random forest patches
        var generator = new ForestPatchesGenerator(
            width, height,
            forestCoverage: 0.2, // 20% forest coverage
            lightWoodsProbability: 0.7); // 70% chance of light woods vs heavy woods
            
        _battleMap = BattleMap.GenerateMap(width, height, generator);
    }

    public BattleMap BattleMap => _battleMap;
    public IImageService ImageService => _imageService;
}
