using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Sanet.MekForge.Core.Models.Game.Combat;
using Sanet.MekForge.Core.Models.Game.Players;

namespace Sanet.MekForge.Core.Models.Game;

public class ClientGame : BaseGame
{
    private readonly Subject<GameCommand> _commandSubject = new();
    private readonly List<GameCommand> _commandLog = [];

    public IObservable<GameCommand> Commands => _commandSubject.AsObservable();
    public IReadOnlyList<GameCommand> CommandLog => _commandLog;
    
    public ClientGame(BattleMap battleMap, IReadOnlyList<IPlayer> localPlayers,
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IToHitCalculator toHitCalculator)
        : base(battleMap, rulesProvider, commandPublisher, toHitCalculator)
    {
        LocalPlayers = localPlayers;
    }
    
    public IReadOnlyList<IPlayer> LocalPlayers { get; }

    public override void HandleCommand(GameCommand command)
    {
        if (!ShouldHandleCommand(command)) return;
        
        _commandLog.Add(command);
        _commandSubject.OnNext(command);
        
        switch (command)
        {
            case JoinGameCommand joinCmd:
                OnPlayerJoined(joinCmd);
                break;
            case UpdatePlayerStatusCommand playerStatusCommand:
                OnPlayerStatusUpdated(playerStatusCommand);
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
    
    public void SetPlayerReady(IPlayer player)
    {
        var readyCommand = new UpdatePlayerStatusCommand() { PlayerId = player.Id, GameOriginId = Id, PlayerStatus = PlayerStatus.Playing };
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
}