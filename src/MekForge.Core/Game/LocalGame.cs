using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Game;

public class LocalGame : BaseGame
{
    public IPlayer LocalPlayer { get; }
    
    public LocalGame(BattleState battleState, 
        IRulesProvider rulesProvider, ICommandPublisher commandPublisher, IPlayer localPlayer)
        : base(battleState, rulesProvider, commandPublisher)
    {
        LocalPlayer = localPlayer;
        
        CommandPublisher.Subscribe(HandleCommand);
    }
    
    public override void HandleCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinCmd:
                AddPlayer(joinCmd);
                break;
            // Handle other commands
        }
    }
    

    public void JoinGameWithUnits(List<UnitData> units)
    {
        var joinCommand = new JoinGameCommand
        {
            PlayerId = LocalPlayer.Id,
            PlayerName = LocalPlayer.Name,
            Units = units
        };
        if (ValidateCommand(joinCommand))
        {
            CommandPublisher.PublishCommand(joinCommand);
        }
    }
}