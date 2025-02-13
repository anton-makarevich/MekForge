using Shouldly;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Tests.Models.Map.Terrains;

public class ClearTerrainTests
{
    [Fact]
    public void Height_Returns0()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.Height.ShouldBe(0);
    }

    [Fact]
    public void TerrainFactor_Returns1()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.MovementCost.ShouldBe(1);
    }

    [Fact]
    public void Id_ReturnsClear()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.Id.ShouldBe("Clear");
    }
}
