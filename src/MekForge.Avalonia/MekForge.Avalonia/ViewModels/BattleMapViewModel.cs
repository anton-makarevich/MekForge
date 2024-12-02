using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.ViewModels;

public class BattleMapViewModel : BaseViewModel
{
    private readonly BattleMap _battleMap;

    public BattleMapViewModel()
    {
        // For testing, let's generate a simple map
        _battleMap = BattleMap.GenerateMap(10, 10, coordinates =>
        {
            var hex = new Hex(coordinates);
            // Add clear terrain to even rows, light woods to odd rows
            hex.AddTerrain( new ClearTerrain());
            return hex;
        });
    }
}
