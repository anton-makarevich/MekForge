using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class PhysicalAttackPhase : MainGamePhase
{
    public PhysicalAttackPhase(ServerGame game) : base(game)
    {
    }

    protected override GamePhase GetNextPhase() => new EndPhase(Game);

    public override void HandleCommand(GameCommand command)
    {
        if (command is not PhysicalAttackCommand attackCommand) return;
        HandleUnitAction(command, attackCommand.PlayerId);
    }

    protected override void ProcessCommand(GameCommand command)
    {
        var attackCommand = (PhysicalAttackCommand)command;
        var broadcastCommand = attackCommand.CloneWithGameId(Game.Id);
        Game.OnPhysicalAttack(attackCommand);
        Game.CommandPublisher.PublishCommand(broadcastCommand);
    }

    public override PhaseNames Name => PhaseNames.PhysicalAttack;
}
