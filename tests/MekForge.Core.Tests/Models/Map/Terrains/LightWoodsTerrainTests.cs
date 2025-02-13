using Shouldly;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Tests.Models.Map.Terrains;

public class LightWoodsTerrainTests
{
    [Fact]
    public void Height_Returns_2()
    {
        // Arrange
        var terrain = new LightWoodsTerrain();

        // Act & Assert
        terrain.Height.ShouldBe(2);
    }

    [Fact]
    public void TerrainFactor_Returns2()
    {
        // Arrange
        var terrain = new LightWoodsTerrain();

        // Act & Assert
        terrain.MovementCost.ShouldBe(2);
    }

    [Fact]
    public void Id_ReturnsLightWoods()
    {
        // Arrange
        var terrain = new LightWoodsTerrain();

        // Act & Assert
        terrain.Id.ShouldBe("LightWoods");
    }
}
