using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public abstract record GameCommand
{
    public required Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public abstract string Format(ILocalizationService localizationService, IGame game);
}