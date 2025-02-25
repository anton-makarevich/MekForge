using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

public record GunneryAttackModifier : AttackModifier
{
    public override string Format(ILocalizationService localizationService) => 
        string.Format(localizationService.GetString("Modifier_GunnerySkill"), Value);
}
