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
    private ServerGame? _serverGame;

    public GameManager(IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IDiceRoller diceRoller,
        IToHitCalculator toHitCalculator)
    {
        _rulesProvider = rulesProvider;
        _commandPublisher = commandPublisher;
        _diceRoller = diceRoller;
        _toHitCalculator = toHitCalculator;
    }

    public void StartServer(BattleMap battleMap)
    {
        _serverGame = new ServerGame(battleMap, _rulesProvider, _commandPublisher, _diceRoller, _toHitCalculator);
        // Start server in background
        Task.Run(() => _serverGame.Start());
    }
}