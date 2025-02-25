using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Combat.Modifiers;

public class GunneryAttackModifierTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var modifier = new GunneryAttackModifier
        {
            Value = 4
        };
        _localizationService.GetString("Modifier_GunnerySkill").Returns("Gunnery Skill: {0}");

        // Act
        var result = modifier.Format(_localizationService);

        // Assert
        result.ShouldBe("Gunnery Skill: 4");
        _localizationService.Received(1).GetString("Modifier_GunnerySkill");
    }
}
