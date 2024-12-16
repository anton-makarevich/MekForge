using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public abstract class BaseGame : IGame
{
    protected readonly BattleState BattleState;
    protected readonly ICommandPublisher CommandPublisher;
    private readonly List<IPlayer> _players = new();
    private readonly MechFactory _mechFactory;
    public Guid GameId { get; private set; }
    public int Turn { get; protected set; } = 1;
    public virtual Phase TurnPhase { get; protected set; } = Phase.Start;

    protected BaseGame(
        BattleState battleState,
        IRulesProvider rulesProvider,
        ICommandPublisher commandPublisher)
    {
        GameId = Guid.NewGuid(); 
        BattleState = battleState;
        CommandPublisher = commandPublisher;
        _mechFactory = new MechFactory(rulesProvider);
        CommandPublisher.Subscribe(HandleCommand);
    }

    public IReadOnlyList<IPlayer> Players => _players;
    public IEnumerable<Hex> GetHexes()
    {
        return BattleState.GetHexes();
    }

    protected void AddPlayer(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName);
        foreach (var unit in joinGameCommand.Units.Select(unitData => _mechFactory.Create(unitData)))
        {
            player.AddUnit(unit);
        }
        _players.Add(player);
    }
    
    protected void UpdatePlayerStatus(UpdatePlayerStatusCommand updatePlayerStatusCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == updatePlayerStatusCommand.PlayerId);
        if (player == null) return;
        player.Status = updatePlayerStatusCommand.PlayerStatus;
    }
    
    protected bool ValidateCommand(GameCommand command)
    {
        return command switch
        {
            JoinGameCommand joinGameCommand => ValidateJoinCommand(joinGameCommand),
            UpdatePlayerStatusCommand playerStateCommand => ValidatePlayer(playerStateCommand),
            DeployUnitCommand deployUnitCommand => ValidateDeployCommand(deployUnitCommand),
            _ => false
        };
    }

    private bool ValidatePlayer(UpdatePlayerStatusCommand updatePlayerStateCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == updatePlayerStateCommand.PlayerId);
        return player != null;
    }

    protected bool ShouldHandleCommand(GameCommand command)
    {
        return command.GameOriginId != GameId && command.GameOriginId != Guid.Empty;
    }

    private bool ValidateJoinCommand(JoinGameCommand joinCommand)
    {
        return joinCommand.PlayerId != null;
    }

    private bool ValidateDeployCommand(DeployUnitCommand cmd)
    {
        return true; //unit != null && !unit.Position.HasValue;
    }

    public abstract void HandleCommand(GameCommand command);
}