namespace Sanet.MekForge.Core.Models.Game.Commands;

public record PlayerStatusCommand: GameCommand
{
    public required PlayerStatus PlayerStatus { get; init; }
}