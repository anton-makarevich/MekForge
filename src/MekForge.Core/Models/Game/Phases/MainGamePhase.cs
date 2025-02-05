using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public abstract class MainGamePhase : GamePhase
{
    private readonly TurnOrder _turnOrder;
    private int _remainingUnits;

    protected MainGamePhase(ServerGame game) : base(game)
    {
        _turnOrder = new TurnOrder();
    }

    public override void Enter()
    {
        _turnOrder.CalculateOrder(Game.InitiativeOrder);
        SetNextPlayerActive();
    }

    protected void SetNextPlayerActive()
    {
        var nextStep = _turnOrder.GetNextStep();
        if (nextStep == null)
        {
            Game.TransitionToPhase(GetNextPhase());
            return;
        }

        _remainingUnits = nextStep.UnitsToMove;
        Game.SetActivePlayer(nextStep.Player, _remainingUnits);
    }

    protected abstract GamePhase GetNextPhase();

    protected void HandleUnitAction(GameCommand command, Guid playerId)
    {
        if (playerId != Game.ActivePlayer?.Id) return;

        ProcessCommand(command);
        
        _remainingUnits--;
        if (_remainingUnits <= 0)
        {
            SetNextPlayerActive();
            return;
        }
        Game.SetActivePlayer(Game.ActivePlayer, _remainingUnits);
    }

    protected abstract void ProcessCommand(GameCommand command);
}
