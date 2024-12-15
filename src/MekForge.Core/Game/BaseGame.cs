using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Game;

public abstract class BaseGame : IGame
{
    protected readonly BattleState BattleState;
    protected readonly ICommandPublisher CommandPublisher;
    private readonly List<IPlayer> _players = new();
    private readonly MechFactory _mechFactory;
    
    protected BaseGame(BattleState battleState, IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
    {
        BattleState = battleState;
        CommandPublisher = commandPublisher;
        _mechFactory = new MechFactory(rulesProvider);
    }

    public IReadOnlyList<IPlayer> Players => _players;

    protected void AddPlayer(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName);
        foreach (var unit in joinGameCommand.Units.Select(unitData => _mechFactory.Create(unitData)))
        {
            player.AddUnit(unit);
        }
        _players.Add(player);
    }
    
    protected bool ValidateCommand(GameCommand command)
    {
        return command switch
        {
            JoinGameCommand joinGameCommand => ValidateJoinCommand(joinGameCommand),
            DeployUnitCommand deployUnitCommand => ValidateDeployCommand(deployUnitCommand),
            _ => false
        };
    }

    private bool ValidateJoinCommand(JoinGameCommand joinCommand)
    {
        return true;
    }

    private bool ValidateDeployCommand(DeployUnitCommand cmd)
    {
        //var unit = _battleState.GetUnit(cmd.UnitId);
        return true; //unit != null && !unit.Position.HasValue;
    }

    public abstract void HandleCommand(GameCommand command);
}