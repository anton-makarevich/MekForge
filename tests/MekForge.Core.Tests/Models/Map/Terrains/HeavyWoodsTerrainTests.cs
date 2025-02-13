using Shouldly;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Tests.Models.Map.Terrains;

public class HeavyWoodsTerrainTests
{
    [Fact]
    public void Height_Returns2()
    {
        // Arrange
        var terrain = new HeavyWoodsTerrain();

        // Act & Assert
        terrain.Height.ShouldBe(2);
    }

    [Fact]
    public void TerrainFactor_Returns3()
    {
        // Arrange
        var terrain = new HeavyWoodsTerrain();

        // Act & Assert
        terrain.MovementCost.ShouldBe(3);
    }

    [Fact]
    public void Id_ReturnsHeavyWoods()
    {
        // Arrange
        var terrain = new HeavyWoodsTerrain();

        // Act & Assert
        terrain.Id.ShouldBe("HeavyWoods");
    }
}
