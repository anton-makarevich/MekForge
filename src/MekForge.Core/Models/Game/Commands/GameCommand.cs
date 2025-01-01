using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.Services.Localization;
using System.Text.Json;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public abstract record GameCommand
{
    public required Guid GameOriginId { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public abstract string Format(ILocalizationService localizationService, IGame game);

    public GameCommand CloneWithGameId(Guid newGameId)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
        };
        var json = JsonSerializer.Serialize(this, GetType(), options);
        var clone = (GameCommand)JsonSerializer.Deserialize(json, GetType(), options)!;
        clone.GameOriginId = newGameId;
        return clone;
    }
}