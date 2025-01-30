using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class WeaponsAttackPhase : GamePhase
{
    private readonly TurnOrder _turnOrder;
    private int _remainingUnitsToAttack;

    public WeaponsAttackPhase(ServerGame game) : base(game)
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
            Game.TransitionToPhase(new PhysicalAttackPhase(Game));
            return;
        }

        _remainingUnitsToAttack = nextStep.UnitsToMove;
        Game.SetActivePlayer(nextStep.Player, _remainingUnitsToAttack);
    }

    public override void HandleCommand(GameCommand command)
    {
        if (command is not ClientCommand clientCommand) return;
        if (clientCommand.PlayerId != Game.ActivePlayer?.Id) return;

        switch (clientCommand)
        {
            case WeaponConfigurationCommand configCommand:
                Game.OnWeaponConfiguration(configCommand);
                break;
            case WeaponsAttackCommand attackCommand:
                Game.OnWeaponsAttack(attackCommand);
                _remainingUnitsToAttack--;
                if (_remainingUnitsToAttack <= 0)
                {
                    SetNextPlayerActive();
                    return;
                }
                Game.SetActivePlayer(Game.ActivePlayer, _remainingUnitsToAttack);
                break;
        }
    }

    public override PhaseNames Name => PhaseNames.WeaponsAttack;
}
