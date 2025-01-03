using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class MovementPhase : GamePhase
{
    private readonly TurnOrder _turnOrder;
    private int _remainingUnitsToMove;

    public MovementPhase(ServerGame game) : base(game)
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
            Game.TransitionToPhase(new AttackPhase(Game));
            return;
        }

        _remainingUnitsToMove = nextStep.UnitsToMove;
        Game.SetActivePlayer(nextStep.Player,_remainingUnitsToMove);
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is not MoveUnitCommand moveCommand) return;
        if (moveCommand.PlayerId != Game.ActivePlayer?.Id) return;

        Game.OnMoveUnit(moveCommand);
        
        _remainingUnitsToMove--;
        if (_remainingUnitsToMove <= 0)
        {
            SetNextPlayerActive();
            return;
        }
        Game.SetActivePlayer(Game.ActivePlayer, _remainingUnitsToMove);
    }

    public override PhaseNames Name => PhaseNames.Movement;
}
