using System.Reactive.Linq;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;
using System.Reactive.Subjects;

namespace Sanet.MekForge.Core.Models.Game;

public abstract class BaseGame : IGame
{
    protected readonly BattleMap BattleMap;
    internal readonly ICommandPublisher CommandPublisher;
    private readonly List<IPlayer> _players = [];
    private readonly MechFactory _mechFactory;
    
    private PhaseNames _turnPhases = PhaseNames.Start;
    private int _turn = 1;
    private IPlayer? _activePlayer;
    private int _unitsToPlayCurrentStep;

    private readonly Subject<int> _turnSubject = new();
    private readonly Subject<PhaseNames> _phaseSubject = new();
    private readonly Subject<IPlayer?> _activePlayerSubject = new();
    private readonly Subject<int> _unitsToPlaySubject = new();

    public Guid GameId { get; }
    public IObservable<int> TurnChanges => _turnSubject.AsObservable();
    public IObservable<PhaseNames> PhaseChanges => _phaseSubject.AsObservable();
    public IObservable<IPlayer?> ActivePlayerChanges => _activePlayerSubject.AsObservable();
    public IObservable<int> UnitsToPlayChanges => _unitsToPlaySubject.AsObservable();

    public int Turn
    {
        get => _turn;
        protected set
        {
            if (_turn == value) return;
            _turn = value;
            _turnSubject.OnNext(value);
        }
    }

    public virtual PhaseNames TurnPhase
    {
        get => _turnPhases;
        protected set
        {
            if (_turnPhases == value) return;
            _turnPhases = value;
            _phaseSubject.OnNext(value);
            ActivePlayer = null;
            UnitsToPlayCurrentStep = 0;
        }
    }

    public virtual IPlayer? ActivePlayer
    {
        get => _activePlayer;
        protected set
        {
            if (_activePlayer == value) return;
            _activePlayer = value;
            _activePlayerSubject.OnNext(value);
        }
    }

    public int UnitsToPlayCurrentStep
    {
        get => _unitsToPlayCurrentStep;
        protected set
        {
            if (_unitsToPlayCurrentStep == value) return;
            _unitsToPlayCurrentStep = value;
            _unitsToPlaySubject.OnNext(value);
        }
    }

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

    /// <summary>
    /// Gets all valid hex coordinates that can be reached from the given position with given movement points
    /// </summary>
    public IEnumerable<HexCoordinates> GetReachableHexes(HexPosition start, int maxMovementPoints)
    {
        return BattleMap.GetReachableHexes(start, maxMovementPoints)
            .Select(x => x.coordinates);
    }

    internal void OnPlayerJoined(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName,joinGameCommand.Tint);
        foreach (var unit in joinGameCommand.Units.Select(unitData => _mechFactory.Create(unitData)))
        {
            unit.Owner = player;
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
        unit?.Deploy(new HexPosition(new HexCoordinates(command.Position), (HexDirection)command.Direction));
    }
    
    public void OnMoveUnit(MoveUnitCommand moveCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == moveCommand.PlayerId);
        if (player == null) return;
        var unit = player.Units.FirstOrDefault(u => u.Id == moveCommand.UnitId);
        unit?.MoveTo(new HexPosition(new HexCoordinates(moveCommand.Destination),0));
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