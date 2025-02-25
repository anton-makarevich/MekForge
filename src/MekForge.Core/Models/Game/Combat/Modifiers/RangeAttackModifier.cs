using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

public record RangeAttackModifier : AttackModifier
{
    public required WeaponRange Range { get; init; }
    public required int Distance { get; init; }
    public required WeaponType WeaponType { get; init; }

    public override string Format(ILocalizationService localizationService) =>
        string.Format(localizationService.GetString("Modifier_Range"), 
            WeaponType, Distance, Range, Value);
}
