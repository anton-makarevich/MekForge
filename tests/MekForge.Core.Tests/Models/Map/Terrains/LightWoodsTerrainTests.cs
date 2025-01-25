using Shouldly;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Tests.Models.Map.Terrains;

public class LightWoodsTerrainTests
{
    [Fact]
    public void Height_Returns1()
    {
        // Arrange
        var terrain = new LightWoodsTerrain();

        // Act & Assert
        terrain.Height.ShouldBe(1);
    }

    [Fact]
    public void TerrainFactor_Returns2()
    {
        // Arrange
        var terrain = new LightWoodsTerrain();

        // Act & Assert
        terrain.TerrainFactor.ShouldBe(2);
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
