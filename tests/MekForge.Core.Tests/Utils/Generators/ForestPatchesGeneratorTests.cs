using FluentAssertions;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Utils.Generators;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Tests.Utils.Generators;

public class ForestPatchesGeneratorTests
{
    [Fact]
    public void WithZeroForestCoverage_GeneratesOnlyClearTerrain()
    {
        // Arrange
        const int width = 10;
        const int height = 10;
        var generator = new ForestPatchesGenerator(width, height, forestCoverage: 0);

        // Act & Assert
        for (var q = 0; q < width; q++)
        {
            for (var r = 0; r < height; r++)
            {
                var hex = generator.Generate(new HexCoordinates(q, r));
                hex.HasTerrain("Clear").Should().BeTrue();
                hex.HasTerrain("LightWoods").Should().BeFalse();
                hex.HasTerrain("HeavyWoods").Should().BeFalse();
            }
        }
    }

    [Fact]
    public void WithFullForestCoverage_GeneratesOnlyWoodsTerrain()
    {
        // Arrange
        const int width = 10;
        const int height = 10;
        var generator = new ForestPatchesGenerator(
            width, height,
            forestCoverage: 1.0,
            lightWoodsProbability: 1.0); // Force all woods to be light woods for deterministic test

        // Act & Assert
        var hasAnyWoods = false;
        for (var q = 0; q < width; q++)
        {
            for (var r = 0; r < height; r++)
            {
                var hex = generator.Generate(new HexCoordinates(q, r));
                hex.HasTerrain("Clear").Should().BeFalse();
                if (hex.HasTerrain("LightWoods") || hex.HasTerrain("HeavyWoods"))
                {
                    hasAnyWoods = true;
                }
            }
        }
        hasAnyWoods.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0)]  // Left of map
    [InlineData(6, 0)]   // Right of map
    [InlineData(0, -1)]  // Above map
    [InlineData(0, 6)]   // Below map
    public void OutOfBounds_ThrowsException(int q, int r)
    {
        // Arrange
        const int width = 5;
        const int height = 5;
        var generator = new ForestPatchesGenerator(width, height);
        var coordinates = new HexCoordinates(q, r);

        // Act & Assert
        var action = () => generator.Generate(coordinates);
        action.Should().Throw<HexOutsideOfMapBoundariesException>()
            .Which.Should().Match<HexOutsideOfMapBoundariesException>(ex =>
                ex.Coordinates == coordinates &&
                ex.MapWidth == width &&
                ex.MapHeight == height);
    }

    [Fact]
    public void CreatesPatches()
    {
        // Arrange
        const int width = 15;
        const int height = 15;
        var generator = new ForestPatchesGenerator(
            width, height,
            forestCoverage: 0.3,
            lightWoodsProbability: 0.5,
            minPatchSize: 3,
            maxPatchSize: 5);

        // Act
        var hexes = new List<Hex>();
        for (var q = 0; q < width; q++)
        {
            for (var r = 0; r < height; r++)
            {
                hexes.Add(generator.Generate(new HexCoordinates(q, r)));
            }
        }

        // Assert
        // Count hexes with woods
        var woodsHexes = hexes.Count(h => h.HasTerrain("LightWoods") || h.HasTerrain("HeavyWoods"));
        woodsHexes.Should().BeGreaterThan(0);

        // Verify that woods appear in patches by checking for adjacent woods hexes
        var hasAdjacentWoods = false;
        for (var q = 1; q < width - 1; q++)
        {
            for (var r = 1; r < height - 1; r++)
            {
                var currentHex = hexes[q * height + r];
                if (currentHex.HasTerrain("LightWoods") || currentHex.HasTerrain("HeavyWoods"))
                {
                    // Check adjacent hexes
                    var coords = new HexCoordinates(q, r);
                    foreach (var neighbor in coords.GetAdjacentCoordinates())
                    {
                        if (neighbor.Q >= 0 && neighbor.Q < width &&
                            neighbor.R >= 0 && neighbor.R < height)
                        {
                            var neighborHex = hexes[neighbor.Q * height + neighbor.R];
                            if (neighborHex.HasTerrain("LightWoods") || neighborHex.HasTerrain("HeavyWoods"))
                            {
                                hasAdjacentWoods = true;
                                break;
                            }
                        }
                    }
                }
                if (hasAdjacentWoods) break;
            }
            if (hasAdjacentWoods) break;
        }
        hasAdjacentWoods.Should().BeTrue();
    }
}
