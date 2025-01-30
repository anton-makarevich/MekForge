using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class PhysicalAttackPhase : GamePhase
{
    private readonly TurnOrder _turnOrder;
    private int _remainingUnitsToAttack;

    public PhysicalAttackPhase(ServerGame game) : base(game)
    {
        _turnOrder = new TurnOrder();
    }

    public override void Enter()
    {
        _turnOrder.CalculateOrder(Game.InitiativeOrder);
        SetNextPlayerActive();
    }

    private void SetNextPlayerActive()
    {
        var nextStep = _turnOrder.GetNextStep();
        if (nextStep == null)
        {
            Game.TransitionToPhase(new EndPhase(Game));
            return;
        }

        _remainingUnitsToAttack = nextStep.UnitsToMove;
        Game.SetActivePlayer(nextStep.Player, _remainingUnitsToAttack);
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is not PhysicalAttackCommand attackCommand) return;
        if (attackCommand.PlayerId != Game.ActivePlayer?.Id) return;

        Game.OnPhysicalAttack(attackCommand);
        
        _remainingUnitsToAttack--;
        if (_remainingUnitsToAttack <= 0)
        {
            SetNextPlayerActive();
            return;
        }
        Game.SetActivePlayer(Game.ActivePlayer, _remainingUnitsToAttack);
    }

    public override PhaseNames Name => PhaseNames.PhysicalAttack;
}
