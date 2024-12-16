namespace Sanet.MekForge.Core.Models.Game.Commands.Server;

public record ChangePhaseCommand : GameCommand
{
    public Phase Phase { get; init; }
}