using System.Collections.ObjectModel;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MakaMek.Core.Data.Units;
using Sanet.MakaMek.Core.Models.Game;
using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Players;
using Sanet.MakaMek.Core.Models.Game.Transport;
using Sanet.MakaMek.Core.Models.Map;
using Sanet.MakaMek.Core.Models.Map.Terrains;
using Sanet.MakaMek.Core.Utils.Generators;
using Sanet.MakaMek.Core.Utils.TechRules;
using Sanet.MakaMek.Core.ViewModels.Wrappers;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MakaMek.Core.ViewModels;

public class NewGameViewModel : BaseViewModel
{
    private int _mapWidth = 15;
    private int _mapHeight = 17;
    private int _forestCoverage = 20;
    private int _lightWoodsPercentage = 30;
    private bool _enableLan;
    private string? _serverUrl;

    private readonly ObservableCollection<PlayerViewModel> _players= [];

    public NewGameViewModel(IGameManager gameManager, IRulesProvider rulesProvider, ICommandPublisher commandPublisher,
        IToHitCalculator toHitCalculator)
    {
        _gameManager = gameManager;
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _toHitCalculator = toHitCalculator;
    }

    public string MapWidthLabel => "Map Width";
    public string MapHeightLabel => "Map Height";
    public string ForestCoverageLabel => "Forest Coverage";
    public string LightWoodsLabel => "Light Woods Percentage";

    private IEnumerable<UnitData> _availableUnits=[];


    private readonly IGameManager _gameManager;
    private readonly IRulesProvider _rulesProvider;
    private readonly ICommandPublisher _commandPublisher;
    private readonly IToHitCalculator _toHitCalculator;


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

    public bool CanStartGame => Players.Count > 0 && Players.All(p => p.Units.Count > 0);
    
    public bool EnableLan
    {
        get => _enableLan;
        set
        {
            bool oldValue = _enableLan;
            SetProperty(ref _enableLan, value);
            
            // If value changed to true, update server URL
            if (oldValue != value && value)
            {
                // When LAN is enabled, get the server URL
                UpdateServerUrl();
            }
        }
    }

    private string? ServerUrl
    {
        get => _serverUrl;
        set => SetProperty(ref _serverUrl, value);
    }
    
    /// <summary>
    /// Gets a formatted server address for display (host:port)
    /// </summary>
    public string? ServerIpAddress
    {
        get
        {
            if (string.IsNullOrEmpty(ServerUrl))
                return null;
                
            // Extract host from the URL
            var uri = new Uri(ServerUrl);
            return $"{uri.Host}";
        }
    }
    
    private void UpdateServerUrl()
    {
        ServerUrl = _gameManager.GetLanServerAddress();
    }


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
        var localBattleMap = BattleMap.CreateFromData(hexDataList);
        
        // Start the server with LAN enabled if requested
        _gameManager.StartServer(localBattleMap, EnableLan);
        
        var localGame = new ClientGame(
            localBattleMap,
            Players.Select(vm=>vm.Player).ToList(),
            _rulesProvider,
            _commandPublisher, _toHitCalculator);

        var battleMapViewModel = NavigationService.GetViewModel<BattleMapViewModel>();
        battleMapViewModel.Game = localGame;

        foreach (var playerViewModel in Players)
        {
            localGame.JoinGameWithUnits(playerViewModel.Player, playerViewModel.Units.ToList());
        }
        
        await NavigationService.NavigateToViewModelAsync(battleMapViewModel);
    });

    public void InitializeUnits(List<UnitData> units)
    {
        // Logic to load available units for selection
        _availableUnits =units;
    }

    public ObservableCollection<PlayerViewModel> Players => _players;

    public ICommand AddPlayerCommand => new AsyncCommand(AddPlayer);

    private Task AddPlayer()
    {
        if (!CanAddPlayer) return Task.CompletedTask; 
        var newPlayer = new Player(Guid.NewGuid(), $"Player {_players.Count + 1}", GetNextTilt());
        var playerViewModel = new PlayerViewModel(
            newPlayer,
            _availableUnits,
            () => { NotifyPropertyChanged(nameof(CanStartGame));});
        _players.Add(playerViewModel);
        NotifyPropertyChanged(nameof(CanAddPlayer));
        NotifyPropertyChanged(nameof(CanStartGame));

        return Task.CompletedTask;
    }

    private string GetNextTilt()
    {
        return Players.Count switch
        {
            0 => "#FFFFFF", // White
            1 => "#FF0000", // Red
            2 => "#0000FF", // Blue
            3 => "#FFFF00", // Yellow
            _ => "#FFFFFF"
        };
    }
    
    public bool CanAddPlayer => _players.Count < 4; // Limit to 4 players for now
}
