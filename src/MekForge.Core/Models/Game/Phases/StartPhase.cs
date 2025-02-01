using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class StartPhase : GamePhase
{
    public StartPhase(ServerGame game) : base(game) { }

    public override void HandleCommand(GameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinGameCommand:
                var broadcastJoinCommand = joinGameCommand.CloneWithGameId(Game.Id);
                Game.OnPlayerJoined(joinGameCommand);
                Game.CommandPublisher.PublishCommand(broadcastJoinCommand);
                break;
            case UpdatePlayerStatusCommand playerStatusCommand:
                var broadcastStatusCommand = playerStatusCommand.CloneWithGameId(Game.Id);
                Game.OnPlayerStatusUpdated(playerStatusCommand);
                Game.CommandPublisher.PublishCommand(broadcastStatusCommand);
                if (AllPlayersReady())
                {
                    Game.TransitionToPhase(new DeploymentPhase(Game));
                }
                break;
        }
    }

    private bool AllPlayersReady()
    {
        return Game.Players.Count > 0 && 
               Game.Players.Count(p => p.Status == PlayerStatus.Playing) == Game.Players.Count;
    }

    public override PhaseNames Name => PhaseNames.Start;
}
