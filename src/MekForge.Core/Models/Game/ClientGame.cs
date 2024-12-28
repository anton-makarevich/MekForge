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
    public ClientGame(BattleMap battleMap, IReadOnlyList<IPlayer> localPlayers,
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleMap, rulesProvider, commandPublisher)
    {
        LocalPlayers = localPlayers;
    }
    
    public IReadOnlyList<IPlayer> LocalPlayers { get; }

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
            case DeployUnitCommand deployUnitCommand:
                OnDeployUnit(deployUnitCommand);
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
}