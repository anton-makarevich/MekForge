using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class ServerGame : BaseGame
{
    public ServerGame(BattleState battleState, IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleState, rulesProvider, commandPublisher)
    {
    }
    
    public override void HandleCommand(GameCommand command)
    {
        if (command is not ClientCommand) return;
        if (!ShouldHandleCommand(command)) return; // Server only accepts commands from players 

        if (!ValidateCommand(command)) return;
        ExecuteCommand(command);

        command.GameOriginId = this.GameId; // Set the GameOriginId before publishing
        CommandPublisher.PublishCommand(command); // Broadcast to all clients
    }
    
    private void ExecuteCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinGameCommand:
                AddPlayer(joinGameCommand);
                break;
            case UpdatePlayerStatusCommand playerStatusCommand:
                UpdatePlayerStatus(playerStatusCommand);
                if (TurnPhase == Phase.Start
                    && Players.Count(p => p.Status == PlayerStatus.Playing) == Players.Count)
                {
                    NextPhase();
                }
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

    public override Phase TurnPhase
    {
        get => base.TurnPhase;
        protected set
        {
            base.TurnPhase = value;
            var command = new ChangePhaseCommand
            {
                GameOriginId = this.GameId,
                Phase = value
            };
            CommandPublisher.PublishCommand(command);
        }
    }

    private bool _isGameOver = false;
    private void NextPhase()
    {
        if (TurnPhase == Phase.End)
        {
            Turn++;
        }; 
        TurnPhase = TurnPhase switch
        {
            Phase.Start => Phase.Deployment,
            Phase.Deployment => Phase.Initiative,
            Phase.Initiative => Phase.Movement,
            Phase.Movement => Phase.Attack,
            Phase.Attack => Phase.End,
            Phase.End => Phase.Deployment,
            
            _ => TurnPhase
        };
    }
}