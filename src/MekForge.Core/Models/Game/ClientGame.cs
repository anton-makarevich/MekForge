using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Sanet.MekForge.Core.Data.Units;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Game.Phases;

namespace Sanet.MekForge.Core.Models.Game;

public sealed class ClientGame : BaseGame
{
    private readonly Subject<IGameCommand> _commandSubject = new();
    private readonly List<IGameCommand> _commandLog = [];

    public IObservable<IGameCommand> Commands => _commandSubject.AsObservable();
    public IReadOnlyList<IGameCommand> CommandLog => _commandLog;
    
    public ClientGame(BattleMap battleMap, IReadOnlyList<IPlayer> localPlayers,
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IToHitCalculator toHitCalculator)
        : base(battleMap, rulesProvider, commandPublisher, toHitCalculator)
    {
        LocalPlayers = localPlayers;
        TurnPhase = PhaseNames.Start;
        SetFirstJoiningLocalPlayerAsActive();
    }
    
    public IReadOnlyList<IPlayer> LocalPlayers { get; }

    public override void HandleCommand(IGameCommand command)
    {
        if (!ShouldHandleCommand(command)) return;
        
        _commandLog.Add(command);
        _commandSubject.OnNext(command);
        
        switch (command)
        {
            case JoinGameCommand joinCmd:
                OnPlayerJoined(joinCmd);
                break;
            case UpdatePlayerStatusCommand statusCommand:
                OnPlayerStatusUpdated(statusCommand);
                // If we're in the Start phase and the player who just got updated was the active player
                if (TurnPhase == PhaseNames.Start && 
                    ActivePlayer != null && 
                    statusCommand.PlayerId == ActivePlayer.Id &&
                    statusCommand.PlayerStatus == PlayerStatus.Playing)
                {
                    // Set the next joining local player as active
                    SetFirstJoiningLocalPlayerAsActive();
                }
                break;
            case ChangePhaseCommand changePhaseCommand:
                TurnPhase = changePhaseCommand.Phase;
                break;
            case ChangeActivePlayerCommand changeActivePlayerCommand:
                var player = Players.FirstOrDefault(p => p.Id == changeActivePlayerCommand.PlayerId);
                ActivePlayer = player;
                UnitsToPlayCurrentStep = changeActivePlayerCommand.UnitsToPlay;
                break;
            case DeployUnitCommand deployUnitCommand:
                OnDeployUnit(deployUnitCommand);
                break;
            case MoveUnitCommand moveUnitCommand:
                OnMoveUnit(moveUnitCommand);
                break;
            case WeaponConfigurationCommand weaponConfigurationCommand:
                OnWeaponConfiguration(weaponConfigurationCommand);
                break;
            case WeaponAttackDeclarationCommand weaponAttackDeclarationCommand:
                OnWeaponsAttack(weaponAttackDeclarationCommand);
                break;
            case WeaponAttackResolutionCommand attackResolutionCommand:
                OnWeaponsAttackResolution(attackResolutionCommand);
                break;
            case HeatUpdatedCommand heatUpdateCommand:
                OnHeatUpdate(heatUpdateCommand);
                break;
        }
    }
    
    private void SetFirstJoiningLocalPlayerAsActive()
    {
        // Find the first local player with Joining status
        var nextPlayer = LocalPlayers.FirstOrDefault(p => p.Status == PlayerStatus.Joining);
        
        if (nextPlayer != null)
        {
            ActivePlayer = nextPlayer;
        }
    }

    public void JoinGameWithUnits(IPlayer player, List<UnitData> units)
    {
        var joinCommand = new JoinGameCommand
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            GameOriginId = Id,
            Tint = player.Tint,
            Units = units
        };
        if (ValidateCommand(joinCommand))
        {
            CommandPublisher.PublishCommand(joinCommand);
        }
    }
    
    public void SetPlayerReady(UpdatePlayerStatusCommand readyCommand)
    {
        if (ValidateCommand(readyCommand))
        {
            CommandPublisher.PublishCommand(readyCommand);
        }
    }

    public void DeployUnit(DeployUnitCommand command)
    {
        if (ActivePlayer == null) return;
        CommandPublisher.PublishCommand(command);
    }

    public void MoveUnit(MoveUnitCommand command)
    {
        if (ActivePlayer == null) return;
        CommandPublisher.PublishCommand(command);
    }

    public void ConfigureUnitWeapons(WeaponConfigurationCommand command)
    {
        if (ActivePlayer == null) return;
        CommandPublisher.PublishCommand(command);
    }

    public void DeclareWeaponAttack(WeaponAttackDeclarationCommand command)
    {
        if (ActivePlayer == null) return;
        CommandPublisher.PublishCommand(command);
    }

    public void EndTurn(TurnEndedCommand command)
    {
        if (ActivePlayer == null) return;
        CommandPublisher.PublishCommand(command);
    }
}