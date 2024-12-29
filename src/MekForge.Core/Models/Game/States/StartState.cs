using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.States;

public class StartState : GameState
{
    public StartState(ServerGame game) : base(game) { }

    public override void HandleCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinGameCommand:
                Game.OnPlayerJoined(joinGameCommand);
                break;
            case UpdatePlayerStatusCommand playerStatusCommand:
                Game.OnPlayerStatusUpdated(playerStatusCommand);
                if (AllPlayersReady())
                {
                    Game.TransitionToState(new DeploymentState(Game));
                }
                break;
        }
    }

    private bool AllPlayersReady()
    {
        return Game.Players.Count > 0 && 
               Game.Players.Count(p => p.Status == PlayerStatus.Playing) == Game.Players.Count;
    }

    public override string Name => "Start";
}
