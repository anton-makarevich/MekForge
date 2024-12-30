using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Phases;

namespace Sanet.MekForge.Core.Models.Game.States;

public class InitiativePhase : GamePhase
{
    private readonly InitiativeOrder _initiativeOrder;

    public InitiativePhase(ServerGame game) : base(game)
    {
        _initiativeOrder = new InitiativeOrder();
    }

    public override void Enter()
    {
        _initiativeOrder.Clear();
        Game.SetActivePlayer(Game.Players[0]);
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is not RollDiceCommand rollCommand) return;
        if (rollCommand.PlayerId != Game.ActivePlayer?.Id) return;

        var roll = Roll2D6();
        _initiativeOrder.AddResult(Game.ActivePlayer, roll);

        // Publish the roll result
        Game.CommandPublisher.PublishCommand(new DiceRolledCommand
        {
            GameOriginId = Game.GameId,
            PlayerId = Game.ActivePlayer.Id,
            Roll = roll
        });

        if (_initiativeOrder.HasTies())
        {
            // If there are ties, only tied players should roll again
            var tiedPlayers = _initiativeOrder.GetTiedPlayers();
            Game.SetActivePlayer(tiedPlayers.First());
        }
        else
        {
            // No ties, get next unrolled player or finish if all have rolled
            var nextPlayer = Game.Players
                .Where(p => p.Status == PlayerStatus.Playing)
                .FirstOrDefault(p => !_initiativeOrder.HasPlayer(p));

            if (nextPlayer != null)
            {
                Game.SetActivePlayer(nextPlayer);
            }
            else
            {
                // Store initiative order in the game
                Game.SetInitiativeOrder(_initiativeOrder.GetOrderedPlayers());
                Game.TransitionToPhase(new MovementPhase(Game));
            }
        }
    }

    private int Roll2D6()
    {
        var rolls = Game.DiceRoller.Roll2D6();
        return rolls.Sum(r => r.Result);
    }

    public override PhaseNames Name => PhaseNames.Initiative;
}
