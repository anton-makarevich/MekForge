using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.States;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Models.Game;

public class ServerGame : BaseGame
{
    private GameState _currentState;

    public ServerGame(BattleMap battleMap, IRulesProvider rulesProvider, ICommandPublisher commandPublisher)
        : base(battleMap, rulesProvider, commandPublisher)
    {
        _currentState = new StartState(this);
    }

    public void TransitionToState(GameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        SetPhase(PhaseFromState(newState));
        _currentState.Enter();
    }

    private Phase PhaseFromState(GameState state) => state.Name switch
    {
        "Start" => Phase.Start,
        "Deployment" => Phase.Deployment,
        "Initiative" => Phase.Initiative,
        "Movement" => Phase.Movement,
        "Attack" => Phase.Attack,
        "End" => Phase.End,
        _ => Phase.Start
    };

    public override void HandleCommand(GameCommand command)
    {
        if (command is not ClientCommand) return;
        if (!ShouldHandleCommand(command)) return;
        if (!ValidateCommand(command)) return;

        _currentState.HandleCommand(command);
        
        // Broadcast the command to all clients
        command.GameOriginId = GameId;
        CommandPublisher.PublishCommand(command);
    }

    public void SetActivePlayer(IPlayer? player)
    {
        ActivePlayer = player;
        if (player != null)
        {
            CommandPublisher.PublishCommand(new ChangeActivePlayerCommand
            {
                GameOriginId = GameId,
                PlayerId = player.Id
            });
        }
    }

    private void SetPhase(Phase phase)
    {
        TurnPhase = phase;
        CommandPublisher.PublishCommand(new ChangePhaseCommand
        {
            GameOriginId = GameId,
            Phase = phase
        });
    }

    public void IncrementTurn()
    {
        Turn++;
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