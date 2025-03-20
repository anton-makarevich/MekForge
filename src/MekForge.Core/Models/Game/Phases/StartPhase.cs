using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class StartPhase(ServerGame game) : GamePhase(game)
{
    public override void HandleCommand(IGameCommand command)
    {
        switch (command)
        {
            case JoinGameCommand joinGameCommand:
                var broadcastJoinCommand = joinGameCommand;
                broadcastJoinCommand.GameOriginId = Game.Id;
                Game.OnPlayerJoined(joinGameCommand);
                Game.CommandPublisher.PublishCommand(broadcastJoinCommand);
                break;
            case UpdatePlayerStatusCommand playerStatusCommand:
                var broadcastStatusCommand = playerStatusCommand;
                broadcastStatusCommand.GameOriginId = Game.Id;
                Game.OnPlayerStatusUpdated(playerStatusCommand);
                Game.CommandPublisher.PublishCommand(broadcastStatusCommand);
                if (AllPlayersReady())
                {
                    Game.TransitionToNextPhase(Name);
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
