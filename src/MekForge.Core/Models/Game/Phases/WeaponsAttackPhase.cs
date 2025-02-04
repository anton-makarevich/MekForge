using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;

namespace Sanet.MekForge.Core.Models.Game.Phases;

public class WeaponsAttackPhase : MainGamePhase
{
    public WeaponsAttackPhase(ServerGame game) : base(game)
    {
    }

    protected override GamePhase GetNextPhase() => new PhysicalAttackPhase(Game);

    public override void HandleCommand(GameCommand command)
    {
        if (command is not ClientCommand clientCommand) return;

        switch (clientCommand)
        {
            case WeaponConfigurationCommand configCommand:
                ProcessWeaponConfiguration(configCommand);
                break;
            case WeaponsAttackCommand attackCommand:
                HandleUnitAction(command, attackCommand.PlayerId);
                break;
        }
    }

    private void ProcessWeaponConfiguration(WeaponConfigurationCommand configCommand)
    {
        var broadcastConfig = configCommand.CloneWithGameId(Game.Id);
        Game.OnWeaponConfiguration(configCommand);
        Game.CommandPublisher.PublishCommand(broadcastConfig);
    }

    protected override void ProcessCommand(GameCommand command)
    {
        var attackCommand = (WeaponsAttackCommand)command;
        var broadcastAttack = attackCommand.CloneWithGameId(Game.Id);
        Game.OnWeaponsAttack(attackCommand);
        Game.CommandPublisher.PublishCommand(broadcastAttack);
    }

    public override PhaseNames Name => PhaseNames.WeaponsAttack;
}
