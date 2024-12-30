using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Phases;

namespace Sanet.MekForge.Core.Models.Game.States;

public class MovementPhase : GamePhase
{
    public MovementPhase(ServerGame game) : base(game) { }

    public override void Enter()
    {
        // TODO: Implement movement phase
        //Game.TransitionToState(new AttackState(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // TODO: Handle movement commands
    }

    public override PhaseNames Name => PhaseNames.Movement;
}
