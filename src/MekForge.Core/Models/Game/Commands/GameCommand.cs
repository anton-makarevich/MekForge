namespace Sanet.MekForge.Core.Models.Game.Commands;

public abstract record GameCommand
{
    public Guid PlayerId { get; init; }
    public Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}