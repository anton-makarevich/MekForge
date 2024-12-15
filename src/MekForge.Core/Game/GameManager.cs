using Sanet.MekForge.Core.Game;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

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

    public void StartServer(BattleState battleState)
    {
        _serverGame = new ServerGame(battleState, _rulesProvider, _commandPublisher);
        // Start server in background
        Task.Run(() => _serverGame.Start());
    }

    // public async Task<IGame> CreateLocalGameAsync(string playerName, List<UnitData> units)
    // {
    //     // Ensure server is running
    //     if (_serverGame == null)
    //     {
    //         StartServer();
    //     }
    //
    //     // Create local game instance
    //     var localBattleState = new BattleState(/* init params */);
    //     var localPlayer = new Player(Guid.NewGuid(), playerName, units);
    //     var localGame = new LocalGame(localBattleState, _commandPublisher, localPlayer);
    //
    //     // Join the game
    //     var joinCommand = new JoinGameCommand
    //     {
    //         PlayerId = localPlayer.Id,
    //         PlayerName = localPlayer.Name,
    //         Units = units
    //     };
    //
    //     _commandPublisher.PublishCommand(joinCommand);
    //
    //     return localGame;
    // }
}
