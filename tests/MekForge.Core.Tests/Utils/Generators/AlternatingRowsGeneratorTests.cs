using FluentAssertions;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Utils.Generators;

namespace Sanet.MekForge.Core.Tests.Utils.Generators;

public class AlternatingRowsGeneratorTests
{
    [Fact]
    public void GeneratesAlternatingTerrains()
    {
        // Arrange
        const int width = 5;
        const int height = 5;
        var evenTerrain = new ClearTerrain();
        var oddTerrain = new LightWoodsTerrain();
        var generator = new AlternatingRowsGenerator(width, height, evenTerrain, oddTerrain);

        // Act & Assert
        for (var r = 1; r < height+1; r++)
        {
            var hex = generator.Generate(new HexCoordinates(2, r));
            if (r % 2 == 0)
            {
                hex.HasTerrain("Clear").Should().BeTrue();
                hex.HasTerrain("LightWoods").Should().BeFalse();
            }
            else
            {
                hex.HasTerrain("Clear").Should().BeFalse();
                hex.HasTerrain("LightWoods").Should().BeTrue();
            }
        }
    }

    [Fact]
    public void OutOfBounds_ShouldThrow()
    {
        // Arrange
        const int width = 5;
        const int height = 5;
        var evenTerrain = new ClearTerrain();
        var oddTerrain = new LightWoodsTerrain();
        var generator = new AlternatingRowsGenerator(width, height, evenTerrain, oddTerrain);
        var coordinates = new HexCoordinates(width + 1, height + 1);
        
        // Act & Assert
        var action = () => generator.Generate(coordinates);
        action.Should().Throw<HexOutsideOfMapBoundariesException>()
            .Which.Should().Match<HexOutsideOfMapBoundariesException>(ex =>
                ex.Coordinates == coordinates &&
                ex.MapWidth == width &&
                ex.MapHeight == height);
    }

    [Fact]
    public void GeneratesConsistentTerrainForSameRow()
    {
        // Arrange
        const int width = 5;
        const int height = 5;
        var evenTerrain = new ClearTerrain();
        var oddTerrain = new LightWoodsTerrain();
        var generator = new AlternatingRowsGenerator(width, height, evenTerrain, oddTerrain);

        // Act & Assert
        for (var r = 1; r < height+1; r++)
        {
            var expectedTerrain = r % 2 == 0 ? "Clear" : "LightWoods";

            // Check all hexes in the same row have the same terrain
            for (var q = 1; q < width+1; q++)
            {
                var hex = generator.Generate(new HexCoordinates(q, r));
                hex.HasTerrain(expectedTerrain).Should().BeTrue();
            }
        }
    }
}
