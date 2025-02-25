using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

public record TargetMovementModifier : AttackModifier
{
    public required int HexesMoved { get; init; }

    public override string Format(ILocalizationService localizationService) =>
        string.Format(localizationService.GetString("Modifier_TargetMovement"), 
            HexesMoved, Value);
}
