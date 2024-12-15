using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Game;

public class ServerGame : BaseGame
{
    public ServerGame(BattleState battleState, IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleState, rulesProvider, commandPublisher)
    {
    }
    
    public override void HandleCommand(GameCommand command)
    {
        if (!ValidateCommand(command)) return;
        ExecuteCommand(command);
        CommandPublisher.PublishCommand(command); // Broadcast to all clients
    }
    
    private void ExecuteCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinGameCommand:
                AddPlayer(joinGameCommand);
                break;
            case MoveUnitCommand moveUnitCommand:
                break;
        }
    }
    
    public async Task Start()
    {
        while (true)
        {
            await Task.Delay(16);
            if (_isGameOver)
                return;
        }
    }
    
    private bool _isGameOver = false;
    
}