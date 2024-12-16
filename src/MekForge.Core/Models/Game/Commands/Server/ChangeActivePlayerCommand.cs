namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record ChangeActivePlayerCommand : GameCommand
{
    public required Guid? PlayerId { get; init; }
}