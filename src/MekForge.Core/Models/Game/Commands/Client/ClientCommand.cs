namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public abstract record ClientCommand: GameCommand
{
    public required Guid PlayerId { get; init; }
}