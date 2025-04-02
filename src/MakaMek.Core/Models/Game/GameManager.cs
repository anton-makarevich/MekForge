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
        IToHitCalculator toHitCalculator, CommandTransportAdapter transportAdapter)
    {
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _diceRoller = diceRoller;
        _toHitCalculator = toHitCalculator;
        _transportAdapter = transportAdapter;
    }

    public void StartServer(BattleMap battleMap)
    {
        _serverGame = new ServerGame(battleMap, _rulesProvider, _commandPublisher, _diceRoller, _toHitCalculator);
        // Start server in background
        _ = Task.Run(() => _serverGame.Start());
    }
    
    public async Task<string?> StartLanServer(BattleMap battleMap)
    {
        // Create and start SignalR host if not already running
        if (_signalRHostService == null)
        {
            _signalRHostService = new SignalRHostService();
            await _signalRHostService.Start(2439); // Use the specified port
            
            // Add the SignalR publisher to the existing transport adapter
            _transportAdapter.AddTransportPublisher(_signalRHostService.Publisher);
        }
        
        // Start the server game if not already started
        if (_serverGame == null)
        {
            _serverGame = new ServerGame(battleMap, _rulesProvider, _commandPublisher, _diceRoller, _toHitCalculator);
            _ = Task.Run(() => _serverGame.Start());
        }
        
        // Return the IP address for display
        return _signalRHostService.ServerIpAddress;
    }
    
    public bool IsLanServerRunning => _signalRHostService?.IsRunning ?? false;
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _signalRHostService?.Dispose();
    }
}