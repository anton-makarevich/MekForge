using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Combat.Modifiers;

public class HeatAttackModifierTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var modifier = new HeatAttackModifier
        {
            Value = 2,
            HeatLevel = 15
        };
        _localizationService.GetString("Modifier_Heat").Returns("Heat Level ({0}): {1}");

        // Act
        var result = modifier.Format(_localizationService);

        // Assert
        result.ShouldBe("Heat Level (15): 2");
        _localizationService.Received(1).GetString("Modifier_Heat");
    }
}
