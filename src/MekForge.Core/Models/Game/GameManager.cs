using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class GameManager : IGameManager
{
    private readonly IRulesProvider _rulesProvider;
    private readonly ICommandPublisher _commandPublisher;
    private ServerGame? _serverGame;

    public GameManager(IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
    {
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
    }

    public void StartServer(BattleMap battleMap)
    {
        _serverGame = new ServerGame(battleMap, _rulesProvider, _commandPublisher);
        // Start server in background
        Task.Run(() => _serverGame.Start());
    }
}