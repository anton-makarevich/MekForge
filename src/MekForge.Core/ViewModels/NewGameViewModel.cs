using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class NewGameViewModel : BaseViewModel
{
    private int _mapWidth = 15;
    private int _mapHeight = 17;
    private int _forestCoverage = 20;
    private int _lightWoodsPercentage = 30;

    public string MapWidthLabel => "Map Width";
    public string MapHeightLabel => "Map Height";
    public string ForestCoverageLabel => "Forest Coverage";
    public string LightWoodsLabel => "Light Woods Percentage";

    public int MapWidth
    {
        get => _mapWidth;
        set => SetProperty(ref _mapWidth, value);
    }

    public int MapHeight
    {
        get => _mapHeight;
        set => SetProperty(ref _mapHeight, value);
    }

    public int ForestCoverage
    {
        get => _forestCoverage;
        set
        {
            SetProperty(ref _forestCoverage, value);
            NotifyPropertyChanged(nameof(IsLightWoodsEnabled));
        }
    }

    public int LightWoodsPercentage
    {
        get => _lightWoodsPercentage;
        set => SetProperty(ref _lightWoodsPercentage, value);
    }

    public bool IsLightWoodsEnabled => _forestCoverage>0;

    public ICommand StartGameCommand => new AsyncCommand(async () =>
    {
        var map = ForestCoverage == 0
            ? BattleMap.GenerateMap(MapWidth, MapHeight, new SingleTerrainGenerator(
                MapWidth, MapHeight, new ClearTerrain()))
            : BattleMap.GenerateMap(MapWidth, MapHeight, new ForestPatchesGenerator(
                MapWidth, MapHeight,
                forestCoverage: ForestCoverage / 100.0,
                lightWoodsProbability: LightWoodsPercentage / 100.0));

        var battleMapViewModel = NavigationService.GetViewModel<BattleMapViewModel>();
        battleMapViewModel.BattleMap = map;

        await NavigationService.NavigateToViewModelAsync(battleMapViewModel);
    });
}
