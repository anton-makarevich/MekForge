using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public abstract record GameCommand
{
    public required Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public abstract string Format(ILocalizationService localizationService, IGame game);

    public GameCommand CloneWithGameId(Guid newGameId)
    {
        var json = GameCommandTypeRegistry.Serialize(this);
        var clone = GameCommandTypeRegistry.Deserialize(json)!;
        clone.GameOriginId = newGameId;
        return clone;
    }
}