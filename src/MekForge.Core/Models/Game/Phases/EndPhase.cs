using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class EndPhase : GamePhase
{
    public EndPhase(ServerGame game) : base(game) { }

    public override void Enter()
    {
        Game.IncrementTurn();
    }

    public override void HandleCommand(IGameCommand command)
    {
        // End state doesn't handle any commands
    }

    public override PhaseNames Name => PhaseNames.End;
}
