using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

public record TerrainAttackModifier : AttackModifier
{
    public required HexCoordinates Location { get; init; }
    public required string TerrainId { get; init; }

    public override string Format(ILocalizationService localizationService) =>
        string.Format(localizationService.GetString("Modifier_Terrain"), 
            TerrainId, Location, Value);
}
