using Sanet.MekForge.Core.Services.Localization;

namespace Sanet.MekForge.Core.Models.Game.Combat.Modifiers;

/// <summary>
/// Base class for all attack modifiers
/// </summary>
public abstract record AttackModifier
{
    public required int Value { get; init; }
    
    public abstract string Format(ILocalizationService localizationService);
}
