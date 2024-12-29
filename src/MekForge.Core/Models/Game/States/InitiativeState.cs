using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.States;

public class InitiativeState : GameState
{
    public InitiativeState(ServerGame game) : base(game) { }

    public override void Enter()
    {
        // TODO: Implement initiative phase
        Game.TransitionToState(new MovementState(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // TODO: Handle initiative commands
    }

    public override string Name => "Initiative";
}
