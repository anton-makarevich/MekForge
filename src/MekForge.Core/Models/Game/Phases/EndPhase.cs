using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Players;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class EndPhase(ServerGame game) : GamePhase(game)
{
    private readonly HashSet<Guid> _playersEndedTurn = new();

    public override void Enter()
    {
        // Don't increment turn here - we'll do it when all players end their turns
        _playersEndedTurn.Clear();
        
        // Set the first player as active
        if (Game.Players.Count > 0 && Game.InitiativeOrder.Count > 0)
        {
            Game.SetActivePlayer(Game.InitiativeOrder[0], 0);
        }
    }

    public override void HandleCommand(IGameCommand command)
    {
        if (command is not TurnEndedCommand turnEndedCommand) return;
        
        // Only accept commands from the active player
        if (Game.ActivePlayer == null || turnEndedCommand.PlayerId != Game.ActivePlayer.Id) return;
        
        // Record that this player has ended their turn
        _playersEndedTurn.Add(turnEndedCommand.PlayerId);
        
        // Find the next player in initiative order who hasn't ended their turn yet
        var nextPlayer = FindNextActivePlayer();
        
        if (nextPlayer != null)
        {
            // There's another player who hasn't ended their turn yet
            Game.SetActivePlayer(nextPlayer, 0);
        }
        else
        {
            // All players have ended their turns, start a new turn
            Game.IncrementTurn();
            
            // Transition to the next phase using the phase manager
            Game.TransitionToNextPhase(Name);
        }
    }
    
    private IPlayer? FindNextActivePlayer()
    {
        foreach (var player in Game.InitiativeOrder)
        {
            // Skip players who have already ended their turn
            if (_playersEndedTurn.Contains(player.Id)) continue;
            
            // Return the first player who hasn't ended their turn yet
            return player;
        }
        
        // All players have ended their turns
        return null;
    }

    public override PhaseNames Name => PhaseNames.End;
}
