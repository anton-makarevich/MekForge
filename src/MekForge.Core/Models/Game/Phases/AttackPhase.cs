using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.States;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class AttackPhase : GamePhase
{
    public AttackPhase(ServerGame game) : base(game) { }

    public override void Enter()
    {
        // TODO: Implement attack phase
        Game.TransitionToPhase(new EndPhase(Game));
    }

    public override void HandleCommand(GameCommand command)
    {
        // TODO: Handle attack commands
    }

    public override PhaseNames Name => PhaseNames.Attack;
}
