using Sanet.MakaMek.Core.Services.Localization;

namespace Sanet.MakaMek.Core.Models.Game.Combat.Modifiers;

public record GunneryAttackModifier : AttackModifier
{
    public override string Format(ILocalizationService localizationService) => 
        string.Format(localizationService.GetString("Modifier_GunnerySkill"), Value);
}
