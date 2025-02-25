using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Combat.Modifiers;

public class AttackerMovementModifierTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var modifier = new AttackerMovementModifier
        {
            Value = 2,
            MovementType = MovementType.Run
        };
        _localizationService.GetString("Modifier_AttackerMovement").Returns("Attacker Movement ({0}): {1}");

        // Act
        var result = modifier.Format(_localizationService);

        // Assert
        result.ShouldBe("Attacker Movement (Run): 2");
        _localizationService.Received(1).GetString("Modifier_AttackerMovement");
    }
}
