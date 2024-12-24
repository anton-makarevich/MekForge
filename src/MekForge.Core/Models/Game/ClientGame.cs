using System.Reactive.Subjects;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class ClientGame : BaseGame
{
    private readonly IReadOnlyList<IPlayer> _localPlayers;
    public readonly Subject<IPlayer> PlayerJoined = new Subject<IPlayer>();


    public ClientGame(BattleMap battleMap, IReadOnlyList<IPlayer> localPlayers,
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleMap, rulesProvider, commandPublisher)
    {
        _localPlayers = localPlayers;
    }
    
    public override void HandleCommand(GameCommand command)
    {
        if (!ShouldHandleCommand(command)) return;
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
                break;
        }
    }
    

    public void JoinGameWithUnits(IPlayer player, List<UnitData> units)
    {
        var joinCommand = new JoinGameCommand
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            GameOriginId = GameId,
            Units = units
        };
        if (ValidateCommand(joinCommand))
        {
            CommandPublisher.PublishCommand(joinCommand);
        }
    }
    
    public void SetPlayerReady(IPlayer player)
    {
        var readyCommand = new UpdatePlayerStatusCommand() { PlayerId = player.Id, GameOriginId = GameId, PlayerStatus = PlayerStatus.Playing };
        if (ValidateCommand(readyCommand))
        {
            CommandPublisher.PublishCommand(readyCommand);
        }
    }

    public bool ActionPossible => ActivePlayer != null
                                   && _localPlayers.Any(lp => lp.Id == ActivePlayer.Id);

    public PlayerActions GetNextClientAction(PlayerActions currentAction)
    {
        if (!ActionPossible) return PlayerActions.None;
        if (TurnPhase == Phase.Deployment)
        {
            if (currentAction == PlayerActions.SelectUnitToDeploy)
            {
                return PlayerActions.SelectHex;
            }
            if (currentAction == PlayerActions.SelectHex)
            {
                return PlayerActions.SelectDirection;
            }
            var hasUnitsToDeploy = ActivePlayer?.Units.Any(u => !u.IsDeployed);
            if (hasUnitsToDeploy == true) return PlayerActions.SelectUnitToDeploy;
        }
        return PlayerActions.None;
    }

    public void DeployUnit(Guid id, HexCoordinates selectedHexCoordinates, HexDirection selectedDirection)
    {
        if (ActivePlayer == null) return;
        var command = new DeployUnitCommand()
        {
            GameOriginId = GameId,
            PlayerId = ActivePlayer.Id,
            UnitId = id,
            Position = selectedHexCoordinates.ToData(),
            Direction = (int)selectedDirection
        };
        CommandPublisher.PublishCommand(command);
    }

    protected override IPlayer OnPlayerJoined(JoinGameCommand joinGameCommand)
    {
        var player = base.OnPlayerJoined(joinGameCommand);
        PlayerJoined.OnNext(player);
        return player;
    }
}