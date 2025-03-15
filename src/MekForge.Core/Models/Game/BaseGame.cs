using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;
using System.Reactive.Subjects;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Utils;

namespace Sanet.MekForge.Core.Models.Game;

public abstract class BaseGame : IGame
{
    
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

    public Guid Id { get; }
    public IObservable<int> TurnChanges => _turnSubject.AsObservable();
    public IObservable<PhaseNames> PhaseChanges => _phaseSubject.AsObservable();
    public IObservable<IPlayer?> ActivePlayerChanges => _activePlayerSubject.AsObservable();
    public IObservable<int> UnitsToPlayChanges => _unitsToPlaySubject.AsObservable();
    public BattleMap BattleMap { get; }
    public IToHitCalculator ToHitCalculator { get; }
    public IRulesProvider RulesProvider { get; }
    
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
        ICommandPublisher commandPublisher,
        IToHitCalculator toHitCalculator)
    {
        Id = Guid.NewGuid(); 
        BattleMap = battleMap;
        RulesProvider = rulesProvider;
        CommandPublisher = commandPublisher;
        _mechFactory = new MechFactory(rulesProvider);
        ToHitCalculator = toHitCalculator;
        CommandPublisher.Subscribe(HandleCommand);
    }

    public IReadOnlyList<IPlayer> Players => _players;

    internal void OnPlayerJoined(JoinGameCommand joinGameCommand)
    {
        var player = new Player(joinGameCommand.PlayerId, joinGameCommand.PlayerName,joinGameCommand.Tint);
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
        unit?.Deploy(new HexPosition(new HexCoordinates(command.Position), (HexDirection)command.Direction));
    }
    
    public void OnMoveUnit(MoveUnitCommand moveCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == moveCommand.PlayerId);
        if (player == null) return;
        var unit = player.Units.FirstOrDefault(u => u.Id == moveCommand.UnitId);
        unit?.Move(
            moveCommand.MovementType,
            moveCommand.MovementPath);
    }
    
    public void OnWeaponConfiguration(WeaponConfigurationCommand configCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == configCommand.PlayerId);
        if (player == null) return;

        var unit = player.Units.FirstOrDefault(u => u.Id == configCommand.UnitId);
        if (unit == null) return;

        switch (configCommand.Configuration.Type)
        {
            case WeaponConfigurationType.TorsoRotation when unit is Mech mech:
                mech.RotateTorso((HexDirection)configCommand.Configuration.Value);
                break;
            case WeaponConfigurationType.ArmsFlip:
                // Handle arms flip when implemented
                break;
        }
    }
    
    public void OnWeaponsAttack(WeaponAttackDeclarationCommand attackCommand)
    {
        // Find the attacking player
        var player = _players.FirstOrDefault(p => p.Id == attackCommand.PlayerId);
        if (player == null) return;
        
        // Find the attacking unit
        var attackerUnit = player.Units.FirstOrDefault(u => u.Id == attackCommand.AttackerId);
        if (attackerUnit == null) return;
        
        // Find all target units
        var targetIds = attackCommand.WeaponTargets.Select(wt => wt.TargetId).Distinct().ToList();
        var targetUnits = _players
            .SelectMany(p => p.Units)
            .Where(u => targetIds.Contains(u.Id))
            .ToList();
        
        // Declare the weapon attack
        attackerUnit.DeclareWeaponAttack(attackCommand.WeaponTargets, targetUnits);
    }
    
    public void OnWeaponsAttackResolution(WeaponAttackResolutionCommand attackResolutionCommand)
    {
        // Find the attacking unit
        var attackerUnit = _players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == attackResolutionCommand.AttackerId);
            
        if (attackerUnit == null) return;
        // Fire the weapon from the attacker unit
        attackerUnit.FireWeapon(attackResolutionCommand.WeaponData);
        // Find the target unit with the target Id
        var targetUnit = _players
            .SelectMany(p => p.Units)
            .FirstOrDefault(u => u.Id == attackResolutionCommand.TargetId);
        
        if (targetUnit == null) return;
        
        // Apply damage to the target unit using the hit locations data
        if (attackResolutionCommand.ResolutionData is { IsHit: true, HitLocationsData: not null })
        {
            targetUnit.ApplyDamage(attackResolutionCommand.ResolutionData.HitLocationsData.HitLocations);
        }
    }
    
    public void OnPhysicalAttack(PhysicalAttackCommand attackCommand)
    {
        Console.WriteLine("physical attack");
    }
    
    protected bool ValidateCommand(IGameCommand command)
    {
        return command switch
        {
            JoinGameCommand joinGameCommand => ValidateJoinCommand(joinGameCommand),
            UpdatePlayerStatusCommand playerStateCommand => ValidatePlayer(playerStateCommand),
            DeployUnitCommand deployUnitCommand => ValidateDeployCommand(deployUnitCommand),
            MoveUnitCommand => true,
            WeaponConfigurationCommand => true,
            WeaponAttackDeclarationCommand=> true,
            WeaponAttackResolutionCommand => true,
            _ => false
        };
    }

    private bool ValidatePlayer(UpdatePlayerStatusCommand updatePlayerStateCommand)
    {
        var player = _players.FirstOrDefault(p => p.Id == updatePlayerStateCommand.PlayerId);
        return player != null;
    }

    protected bool ShouldHandleCommand(IGameCommand command)
    {
        return command.GameOriginId != Id && command.GameOriginId != Guid.Empty;
    }

    private bool ValidateJoinCommand(JoinGameCommand joinCommand)
    {
        return true;
    }

    private bool ValidateDeployCommand(DeployUnitCommand cmd)
    {
        return true; //unit != null && !unit.Position.HasValue;
    }

    public abstract void HandleCommand(IGameCommand command);
}