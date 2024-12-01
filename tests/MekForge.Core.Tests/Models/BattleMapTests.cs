using FluentAssertions;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Terrains;

namespace MekForge.Core.Tests.Models;

public class BattleMapTests
{
    [Fact]
    public void AddHex_StoresHexInMap()
    {
        // Arrange
        var map = new BattleMap();
        var hex = new Hex(new HexCoordinates(0, 0));

        // Act
        map.AddHex(hex);

        // Assert
        map.GetHex(hex.Coordinates).Should().Be(hex);
    }

    [Fact]
    public void FindPath_WithClearTerrain_FindsShortestPath()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);
        var target = new HexCoordinates(2, 0);

        // Add hexes with clear terrain
        for (var q = 0; q <= 2; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 0));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Act
        var path = map.FindPath(start, target, 10);

        // Assert
        path.Should().NotBeNull();
        path!.Count.Should().Be(2); // Should be [1,0] and [2,0]
        path.Should().ContainInOrder(
            new HexCoordinates(1, 0),
            new HexCoordinates(2, 0)
        );
    }

    [Fact]
    public void FindPath_WithHeavyWoods_TakesLongerPath()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);
        var target = new HexCoordinates(2, 0);

        // Add clear terrain path around
        for (var q = 0; q <= 2; q++)
        {
            var hex = new Hex(new HexCoordinates(q, -1));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Add heavy woods on direct path
        for (var q = 0; q <= 2; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 0));
            hex.AddTerrain(new HeavyWoodsTerrain());
            map.AddHex(hex);
        }

        // Act
        var path = map.FindPath(start, target, 10);

        // Assert
        path.Should().NotBeNull();
        path!.Should().Contain(new HexCoordinates(1, -1)); // Should go through clear terrain
    }

    [Fact]
    public void GetReachableHexes_WithClearTerrain_ReturnsCorrectHexes()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);

        // Add clear terrain in a 2-hex radius
        foreach (var hex in start.GetCoordinatesInRange(2))
        {
            var mapHex = new Hex(hex);
            mapHex.AddTerrain(new ClearTerrain());
            map.AddHex(mapHex);
        }

        // Act
        var reachable = map.GetReachableHexes(start, 2).ToList();

        // Assert
        reachable.Count.Should().Be(18); // 6 hexes at distance 1, 12 at distance 2
        reachable.All(h => h.cost <= 2).Should().BeTrue();
        reachable.Count(h => h.cost == 1).Should().Be(6); // 6 adjacent hexes
        reachable.Count(h => h.cost == 2).Should().Be(12); // 12 hexes at distance 2
    }

    [Fact]
    public void GetReachableHexes_WithMixedTerrain_ConsidersTerrainCosts()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);

        // Add clear terrain hex
        var clearHex = new Hex(new HexCoordinates(1, 0));
        clearHex.AddTerrain(new ClearTerrain());
        map.AddHex(clearHex);

        // Add heavy woods hex
        var woodsHex = new Hex(new HexCoordinates(0, 1));
        woodsHex.AddTerrain(new HeavyWoodsTerrain());
        map.AddHex(woodsHex);

        // Act
        var reachable = map.GetReachableHexes(start, 2).ToList();

        // Assert
        reachable.Count.Should().Be(1); // Only the clear hex should be reachable
        reachable.First().coordinates.Should().Be(clearHex.Coordinates);
    }

    [Fact]
    public void HasLineOfSight_WithClearPath_ReturnsTrue()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);
        var end = new HexCoordinates(3, 0);

        // Add clear terrain hexes
        for (var q = 0; q <= 3; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 0));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Act
        var hasLos = map.HasLineOfSight(start, end);

        // Assert
        hasLos.Should().BeTrue();
    }

    [Fact]
    public void HasLineOfSight_WithBlockingTerrain_ReturnsFalse()
    {
        // Arrange
        var map = new BattleMap();
        var start = new HexCoordinates(0, 0);
        var end = new HexCoordinates(3, 0);

        // Add clear terrain hexes
        for (var q = 0; q <= 3; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 0));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Add blocking heavy woods in the middle
        var blockingHex = new Hex(new HexCoordinates(1, 0), 2); // Higher base level
        blockingHex.AddTerrain(new HeavyWoodsTerrain()); // Adds 2 more levels
        map.AddHex(blockingHex);

        // Act
        var hasLos = map.HasLineOfSight(start, end);

        // Assert
        hasLos.Should().BeFalse();
    }

    [Fact]
    public void GenerateMap_CreatesCorrectSizedMap()
    {
        // Arrange
        const int width = 5;
        const int height = 4;

        // Act
        var map = BattleMap.GenerateMap(width, height, coordinates => 
            new Hex(coordinates));

        // Assert
        map.Width.Should().Be(width);
        map.Height.Should().Be(height);

        // Check if all hexes are created
        for (var r = 0; r < height; r++)
        {
            var qStart = r % 2 == 0 ? 0 : -1;
            var qEnd = width + (r % 2 == 0 ? 0 : -1);
            
            for (var q = qStart; q < qEnd; q++)
            {
                var hex = map.GetHex(new HexCoordinates(q, r));
                hex.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public void GenerateMap_WithTerrainGenerator_CreatesCorrectTerrain()
    {
        // Arrange
        const int width = 3;
        const int height = 3;

        // Act
        var map = BattleMap.GenerateMap(width, height, coordinates =>
        {
            var hex = new Hex(coordinates);
            // Add clear terrain to even rows, light woods to odd rows
            hex.AddTerrain(coordinates.R % 2 == 0 
                ? new ClearTerrain() 
                : new LightWoodsTerrain());
            return hex;
        });

        // Assert
        // Check even rows have clear terrain
        for (var q = 0; q < width; q++)
        {
            var hex = map.GetHex(new HexCoordinates(q, 0));
            hex!.HasTerrain("Clear").Should().BeTrue();
        }

        // Check odd rows have light woods
        var qStart = -1; // Odd rows are offset
        for (var q = qStart; q < width - 1; q++)
        {
            var hex = map.GetHex(new HexCoordinates(q, 1));
            hex!.HasTerrain("LightWoods").Should().BeTrue();
        }
    }
}
