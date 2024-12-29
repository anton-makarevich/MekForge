using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.States;

public class AttackState : GameState
{
    public AttackState(ServerGame game) : base(game) { }

    public override void Enter()
    {
        // TODO: Implement attack phase
        Game.TransitionToState(new EndState(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // TODO: Handle attack commands
    }

    public override string Name => "Attack";
}
