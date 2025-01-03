using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public abstract class BaseGame : IGame
{
    protected readonly BattleMap BattleMap;
    internal readonly ICommandPublisher CommandPublisher;
    private readonly List<IPlayer> _players = [];
    private readonly MechFactory _mechFactory;
    private PhaseNames _turnPhases = PhaseNames.Start;
    public Guid GameId { get; }
    public int Turn { get; protected set; } = 1;

    public virtual PhaseNames TurnPhase
    {
        get => _turnPhases;
        protected set
        {
            _turnPhases = value;
            ActivePlayer = null;
            UnitsToPlayCurrentStep = 0;
        }
    }

    public virtual IPlayer? ActivePlayer { get; protected set; }
    public int UnitsToPlayCurrentStep { get; protected set; }

    protected BaseGame(
        BattleMap battleMap,
        IRulesProvider rulesProvider,
        ICommandPublisher commandPublisher)
    {
        GameId = Guid.NewGuid(); 
        BattleMap = battleMap;
        CommandPublisher = commandPublisher;
        _mechFactory = new MechFactory(rulesProvider);
        CommandPublisher.Subscribe(HandleCommand);
    }

    public IReadOnlyList<IPlayer> Players => _players;
    public IEnumerable<Hex> GetHexes()
    {
        return BattleMap.GetHexes();
    }

    internal void OnPlayerJoined(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName);
        foreach (var unit in joinGameCommand.Units.Select(unitData => _mechFactory.Create(unitData)))
        {
            player.AddUnit(unit);
        }
        _players.Add(player);
    }
    
    internal void OnPlayerStatusUpdated(UpdatePlayerStatusCommand updatePlayerStatusCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == updatePlayerStatusCommand.PlayerId);
        if (player == null) return;
        player.Status = updatePlayerStatusCommand.PlayerStatus;
    }

    internal void OnDeployUnit(DeployUnitCommand command)
    {
        var player = _players.FirstOrDefault(p => p.Id == command.PlayerId);
        if (player == null) return;
        var unit = player.Units.FirstOrDefault(u => u.Id == command.UnitId && !u.IsDeployed);
        unit?.Deploy(new HexCoordinates(command.Position), (HexDirection)command.Direction);
    }
    
    public void OnMoveUnit(MoveUnitCommand moveCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == moveCommand.PlayerId);
        if (player == null) return;
        var unit = player.Units.FirstOrDefault(u => u.Id == moveCommand.UnitId);
        unit?.MoveTo(new HexCoordinates(moveCommand.Destination));
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
        return true;
    }

    private bool ValidateDeployCommand(DeployUnitCommand cmd)
    {
        return true; //unit != null && !unit.Position.HasValue;
    }

    public abstract void HandleCommand(GameCommand command);
}