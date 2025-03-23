using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class EndPhase(ServerGame game) : GamePhase(game)
{
    private readonly HashSet<Guid> _playersEndedTurn = new();

    public override void Enter()
    {
        // Don't increment turn here - we'll do it when all players end their turns
        _playersEndedTurn.Clear();
    }

    public override void HandleCommand(IGameCommand command)
    {
        if (command is not TurnEndedCommand turnEndedCommand) return;
        
        // Verify the player is in the game
        var player = Game.Players.FirstOrDefault(p => p.Id == turnEndedCommand.PlayerId);
        if (player == null) return;
        
        // Record that this player has ended their turn
        _playersEndedTurn.Add(turnEndedCommand.PlayerId);
        
        // Check if all players have ended their turn
        if (HaveAllPlayersEndedTurn())
        {
            // All players have ended their turns, start a new turn
            Game.IncrementTurn();
            
            // Transition to the next phase using the phase manager
            Game.TransitionToNextPhase(Name);
        }
    }
    
    private bool HaveAllPlayersEndedTurn()
    {
        // Check if all players in the game have ended their turn
        return Game.Players.All(player => _playersEndedTurn.Contains(player.Id));
    }

    public override PhaseNames Name => PhaseNames.End;
}
