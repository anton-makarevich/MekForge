using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.States;

public class EndState : GameState
{
    public EndState(ServerGame game) : base(game) { }

    public override void Enter()
    {
        Game.IncrementTurn();
        Game.TransitionToState(new DeploymentState(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // End state doesn't handle any commands
    }

    public override string Name => "End";
}
