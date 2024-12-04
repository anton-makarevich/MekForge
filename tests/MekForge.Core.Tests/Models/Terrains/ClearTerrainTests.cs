using FluentAssertions;
using Sanet.MekForge.Core.Models.Terrains;

namespace Sanet.MekForge.Core.Tests.Models.Terrains;

public class ClearTerrainTests
{
    [Fact]
    public void Height_Returns0()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.Height.Should().Be(0);
    }

    [Fact]
    public void TerrainFactor_Returns1()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.TerrainFactor.Should().Be(1);
    }

    [Fact]
    public void Id_ReturnsClear()
    {
        // Arrange
        var terrain = new ClearTerrain();

        // Act & Assert
        terrain.Id.Should().Be("Clear");
    }
}
