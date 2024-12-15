namespace Sanet.MekForge.Core.Models.Game.Commands;

public abstract record GameCommand
{
    public required Guid PlayerId { get; init; }
    public required Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}