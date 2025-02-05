using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class MovementPhase : MainGamePhase
{
    public MovementPhase(ServerGame game) : base(game)
    {
    }

    protected override GamePhase GetNextPhase() => new WeaponsAttackPhase(Game);

    public override void HandleCommand(GameCommand command)
    {
        if (command is not MoveUnitCommand moveCommand) return;
        HandleUnitAction(command, moveCommand.PlayerId);
    }

    protected override void ProcessCommand(GameCommand command)
    {
        var moveCommand = (MoveUnitCommand)command;
        var broadcastCommand = moveCommand.CloneWithGameId(Game.Id);
        Game.OnMoveUnit(moveCommand);
        Game.CommandPublisher.PublishCommand(broadcastCommand);
    }

    public override PhaseNames Name => PhaseNames.Movement;
}
