using FluentAssertions;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;

namespace MekForge.Core.Tests.Models;

public class HexTests
{
    [Fact]
    public void Constructor_SetsCoordinatesAndLevel()
    {
        // Arrange
        var coords = new HexCoordinates(1, 2);

        // Act
        var hex = new Hex(coords, 3);

        // Assert
        hex.Coordinates.Should().Be(coords);
        hex.Level.Should().Be(3);
    }

    [Fact]
    public void Constructor_DefaultLevel_IsZero()
    {
        // Arrange & Act
        var hex = new Hex(new HexCoordinates(0, 0));

        // Assert
        hex.Level.Should().Be(0);
    }

    [Fact]
    public void AddTerrain_AddsTerrainToHex()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        var heavyWoods = new HeavyWoodsTerrain();

        // Act
        hex.AddTerrain(heavyWoods);

        // Assert
        hex.HasTerrain("HeavyWoods").Should().BeTrue();
        hex.GetTerrain("HeavyWoods").Should().Be(heavyWoods);
    }

    [Fact]
    public void RemoveTerrain_RemovesTerrainFromHex()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        hex.AddTerrain(new HeavyWoodsTerrain());

        // Act
        hex.RemoveTerrain("HeavyWoods");

        // Assert
        hex.HasTerrain("HeavyWoods").Should().BeFalse();
        hex.GetTerrain("HeavyWoods").Should().BeNull();
    }

    [Fact]
    public void GetTerrains_ReturnsAllTerrains()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0));
        var heavyWoods = new HeavyWoodsTerrain();
        hex.AddTerrain(heavyWoods);

        // Act
        var terrains = hex.GetTerrains().ToList();

        // Assert
        terrains.Should().HaveCount(1);
        terrains.Should().Contain(heavyWoods);
    }

    [Fact]
    public void GetCeiling_ReturnsLevelPlusHighestTerrainHeight()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0), 2);
        hex.AddTerrain(new HeavyWoodsTerrain());

        // Act
        var ceiling = hex.GetCeiling();

        // Assert
        ceiling.Should().Be(4); // Base level (2) + terrain height (2)
    }

    [Fact]
    public void GetCeiling_WithNoTerrain_ReturnsLevel()
    {
        // Arrange
        var hex = new Hex(new HexCoordinates(0, 0), 2);

        // Act
        var ceiling = hex.GetCeiling();

        // Assert
        ceiling.Should().Be(2);
    }
}
