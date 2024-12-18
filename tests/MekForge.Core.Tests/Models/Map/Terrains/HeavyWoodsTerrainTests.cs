using FluentAssertions;
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
        terrain.Height.Should().Be(2);
    }

    [Fact]
    public void TerrainFactor_Returns3()
    {
        // Arrange
        var terrain = new HeavyWoodsTerrain();

        // Act & Assert
        terrain.TerrainFactor.Should().Be(3);
    }

    [Fact]
    public void Id_ReturnsHeavyWoods()
    {
        // Arrange
        var terrain = new HeavyWoodsTerrain();

        // Act & Assert
        terrain.Id.Should().Be("HeavyWoods");
    }
}
