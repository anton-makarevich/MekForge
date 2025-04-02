using Sanet.MakaMek.Core.Models.Game.Combat;
using Sanet.MakaMek.Core.Models.Game.Dice;
using Sanet.MakaMek.Core.Models.Game.Transport;
using Sanet.MakaMek.Core.Models.Map;
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
    private SignalRHostService? _signalRHostService;
    private bool _isDisposed;

    public GameManager(IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IDiceRoller diceRoller,
        IToHitCalculator toHitCalculator, CommandTransportAdapter transportAdapter, SignalRHostService signalRHostService)
    {
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _diceRoller = diceRoller;
        _toHitCalculator = toHitCalculator;
        _transportAdapter = transportAdapter;
        _signalRHostService = signalRHostService;
    }

    public void StartServer(BattleMap battleMap, bool enableLan = false)
    {
        // Start the SignalR host if LAN is enabled and not already running
        if (enableLan && !IsLanServerRunning)
        {
            _ = _signalRHostService?.Start(2439);
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
        // Start the SignalR host if not already running
        if (!IsLanServerRunning)
        {
            _ = _signalRHostService?.Start(2439);
        }
        
        return _signalRHostService?.ServerIpAddress;
    }
    
    public bool IsLanServerRunning => _signalRHostService?.IsRunning ?? false;
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _signalRHostService?.Dispose();
    }
}