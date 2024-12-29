using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.States;

public class MovementState : GameState
{
    public MovementState(ServerGame game) : base(game) { }

    public override void Enter()
    {
        // TODO: Implement movement phase
        Game.TransitionToState(new AttackState(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // TODO: Handle movement commands
    }

    public override string Name => "Movement";
}
