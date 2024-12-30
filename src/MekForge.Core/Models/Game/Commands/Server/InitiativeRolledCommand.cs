namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record InitiativeRolledCommand : GameCommand
{
    public required Guid PlayerId { get; init; }
    public required int Roll { get; init; }
}
