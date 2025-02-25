using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

public record AttackerMovementModifier : AttackModifier
{
    public required MovementType MovementType { get; init; }

    public override string Format(ILocalizationService localizationService) =>
        string.Format(localizationService.GetString("Modifier_AttackerMovement"), 
            MovementType, Value);
}
