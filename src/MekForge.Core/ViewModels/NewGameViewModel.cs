using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Models.Terrains;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class NewGameViewModel : BaseViewModel
{
    private int _mapWidth = 15;
    private int _mapHeight = 17;
    private int _forestCoverage = 20;
    private int _lightWoodsPercentage = 30;
    
    public NewGameViewModel(IGameManager gameManager, IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
    {
        _gameManager = gameManager;
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
    }

    public string MapWidthLabel => "Map Width";
    public string MapHeightLabel => "Map Height";
    public string ForestCoverageLabel => "Forest Coverage";
    public string LightWoodsLabel => "Light Woods Percentage";
    
    private ObservableCollection<UnitData> _availableUnits=[];
    
    private UnitData? _selectedUnit;
    private readonly IGameManager _gameManager;
    private readonly IRulesProvider _rulesProvider;
    private readonly ICommandPublisher _commandPublisher;

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
    
    public bool CanStartGame => SelectedUnit != null;

    public ICommand StartGameCommand => new AsyncCommand(async () =>
    {
        var map = ForestCoverage == 0
            ? BattleMap.GenerateMap(MapWidth, MapHeight, new SingleTerrainGenerator(
                MapWidth, MapHeight, new ClearTerrain()))
            : BattleMap.GenerateMap(MapWidth, MapHeight, new ForestPatchesGenerator(
                MapWidth, MapHeight,
                forestCoverage: ForestCoverage / 100.0,
                lightWoodsProbability: LightWoodsPercentage / 100.0));
        
        var hexDataList = map.GetHexes().Select(hex => hex.ToData()).ToList();
        var localBattleState = new BattleState(BattleMap.CreateFromData(hexDataList));
        
        _gameManager.StartServer(localBattleState);
        var player = new Player(Guid.NewGuid(), "Player 1");
        var localGame = new LocalGame(localBattleState, _rulesProvider, _commandPublisher);

        var battleMapViewModel = NavigationService.GetViewModel<BattleMapViewModel>();
        if (SelectedUnit != null) localGame.JoinGameWithUnits(player,[SelectedUnit.Value]);
        battleMapViewModel.Game = localGame;

        await NavigationService.NavigateToViewModelAsync(battleMapViewModel);
    });

    public ObservableCollection<UnitData> AvailableUnits
    {
        get => _availableUnits;
        set => SetProperty(ref _availableUnits, value);
    }

    public UnitData? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            SetProperty(ref _selectedUnit, value);
            NotifyPropertyChanged(nameof(CanStartGame));
        }
    }

    public void InitializeUnits(List<UnitData> units)
    {
        // Logic to load available units for selection
        AvailableUnits = new ObservableCollection<UnitData>(units);
    }
}
