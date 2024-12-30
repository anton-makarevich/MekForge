namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record DiceRolledCommand : GameCommand
{
    public required Guid PlayerId { get; init; }
    public required int Roll { get; init; }
}
