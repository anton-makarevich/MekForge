using Sanet.MekForge.Avalonia.Services;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private readonly BattleMap _battleMap;
    private readonly IImageService _imageService;

    public BattleMapViewModel(IImageService imageService)
    {
        _imageService = imageService;
        // For testing, let's generate a simple map
        _battleMap = BattleMap.GenerateMap(14, 12, coordinates =>
        {
            var hex = new Hex(coordinates);
            // Add clear terrain to even rows, light woods to odd rows
            hex.AddTerrain(coordinates.R % 2 == 0
                ? new ClearTerrain()
                : new LightWoodsTerrain());
            return hex;
        });
    }

    public BattleMap BattleMap => _battleMap;
    public IImageService ImageService => _imageService;
}
