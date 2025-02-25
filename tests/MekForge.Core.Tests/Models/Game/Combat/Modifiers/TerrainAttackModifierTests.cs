using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Combat.Modifiers;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Combat.Modifiers;

public class TerrainAttackModifierTests
{
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    [Fact]
    public void Format_ShouldFormatCorrectly()
    {
        // Arrange
        var hexCoordinates = new HexCoordinates(3, 4);
        var modifier = new TerrainAttackModifier
        {
            Value = 1,
            TerrainId = "LightWoods",
            Location = hexCoordinates
        };
        _localizationService.GetString("Modifier_Terrain").Returns("{0} at {1}: {2}");

        // Act
        var result = modifier.Format(_localizationService);

        // Assert
        result.ShouldBe("LightWoods at 0304: 1");
        _localizationService.Received(1).GetString("Modifier_Terrain");
    }
}
