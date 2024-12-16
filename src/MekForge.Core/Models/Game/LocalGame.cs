using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class LocalGame : BaseGame
{
    public LocalGame(BattleState battleState, 
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleState, rulesProvider, commandPublisher)
    {
    }
    
    public override void HandleCommand(GameCommand command)
    {
        if (!ShouldHandleCommand(command)) return;
        switch (command)
        {
            case JoinGameCommand joinCmd:
                AddPlayer(joinCmd);
                break;
            case PlayerStatusCommand playerStatusCommand:
                UpdatePlayerStatus(playerStatusCommand);
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
        var readyCommand = new PlayerStatusCommand() { PlayerId = player.Id, GameOriginId = GameId, PlayerStatus = PlayerStatus.Playing };
        if (ValidateCommand(readyCommand))
        {
            CommandPublisher.PublishCommand(readyCommand);
        }
    }
}