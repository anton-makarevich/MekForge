using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Dice;
using Sanet.MakaMek.Core.Models.Map;
using Sanet.MakaMek.Core.Services.Transport;
using Sanet.MakaMek.Core.Utils.TechRules;

namespace Sanet.MakaMek.Core.Models.Game;

public class GameManager : IGameManager
{
    private readonly IRulesProvider _rulesProvider;
    private readonly ICommandPublisher _commandPublisher;
    private readonly IDiceRoller _diceRoller;
    private readonly IToHitCalculator _toHitCalculator;
    private readonly CommandTransportAdapter _transportAdapter;
    private ServerGame? _serverGame;
    private readonly INetworkHostService? _networkHostService;
    private bool _isDisposed;

    public GameManager(IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IDiceRoller diceRoller,
        IToHitCalculator toHitCalculator, CommandTransportAdapter transportAdapter, INetworkHostService networkHostService)
    {
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _diceRoller = diceRoller;
        _toHitCalculator = toHitCalculator;
        _transportAdapter = transportAdapter;
        _networkHostService = networkHostService;
    }

    public void StartServer(BattleMap battleMap, bool enableLan = false)
    {
        // Start the network host if LAN is enabled and not already running
        if (enableLan && !IsLanServerRunning)
        {
            _ = _networkHostService?.Start(2439);
            
            // Add the network publisher to the transport adapter when it's available
            if (_networkHostService?.Publisher != null)
            {
                _transportAdapter.AddPublisher(_networkHostService.Publisher);
            }
        }
        
        // Start the game server if not already running
        if (_serverGame == null)
        {
            _serverGame = new ServerGame(battleMap, _rulesProvider, _commandPublisher, _diceRoller, _toHitCalculator);
            // Start server in background
            _ = Task.Run(() => _serverGame.Start());
        }
    }
    
    public string? GetLanServerAddress()
    {
        // Start the network host if not already running
        if (!IsLanServerRunning)
        {
            return null;
        }
        
        return _networkHostService?.HubUrl;
    }
    
    public bool IsLanServerRunning => _networkHostService?.IsRunning ?? false;
    public bool CanStartLanServer => _networkHostService?.CanStart?? false;

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _networkHostService?.Dispose();
    }
}