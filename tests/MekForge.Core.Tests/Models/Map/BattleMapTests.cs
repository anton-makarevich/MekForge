using FluentAssertions;
using NSubstitute;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;
using Sanet.MekForge.Core.Utils.Generators;

namespace Sanet.MekForge.Core.Tests.Models.Map;

public class BattleMapTests
{
    [Fact]
    public void Constructor_SetsWidthAndHeight()
    {
        // Arrange & Act
        const int width = 5;
        const int height = 4;
        var map = new BattleMap(width, height);

        // Assert
        map.Width.Should().Be(width);
        map.Height.Should().Be(height);
    }

    [Fact]
    public void AddHex_StoresHexInMap()
    {
        // Arrange
        var map = new BattleMap(1, 1);
        var hex = new Hex(new HexCoordinates(1, 1));

        // Act
        map.AddHex(hex);

        // Assert
        map.GetHex(hex.Coordinates).Should().Be(hex);
    }

    [Theory]
    [InlineData(-1, 0)]  // Left of map
    [InlineData(2, 0)]   // Right of map
    [InlineData(0, -1)]  // Above map
    [InlineData(0, 2)]   // Below map
    public void AddHex_OutsideMapBoundaries_ThrowsException(int q, int r)
    {
        // Arrange
        var map = new BattleMap(2, 2);
        var hex = new Hex(new HexCoordinates(q, r));

        // Act & Assert
        var action = () => map.AddHex(hex);
        action.Should().Throw<HexOutsideOfMapBoundariesException>()
            .Which.Should().Match<HexOutsideOfMapBoundariesException>(ex =>
                ex.Coordinates == hex.Coordinates &&
                ex.MapWidth == 2 &&
                ex.MapHeight == 2);
    }

    [Fact]
    public void FindPath_WithClearTerrain_FindsShortestPath()
    {
        // Arrange
        var map = new BattleMap(3, 1);
        var start = new HexCoordinates(1, 1);
        var target = new HexCoordinates(3, 1);

        // Add hexes with clear terrain
        for (var q = 1; q <= 3; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 1));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Act
        var path = map.FindPath(start, target, 10);

        // Assert
        path.Should().NotBeNull();
        path!.Count.Should().Be(2); // Should be [2,1] and [3,1]
        path.Should().ContainInOrder(
            new HexCoordinates(2, 1),
            new HexCoordinates(3, 1)
        );
    }

    [Fact]
    public void FindPath_WithHeavyWoods_TakesLongerPath()
    {
        // Arrange
        var map = new BattleMap(2, 3);
        var start = new HexCoordinates(1, 1);
        var target = new HexCoordinates(2, 3);

        // Add heavy woods on col 2
        for (var r = 1; r <= 3; r++)
        {
            var hex = new Hex(new HexCoordinates(2, r));
            hex.AddTerrain(new HeavyWoodsTerrain());
            map.AddHex(hex);
        }

        // Add clear terrain path through row 1
        for (var r = 1; r <= 3; r++)
        {
            var hex = new Hex(new HexCoordinates(1, r));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Act
        var path = map.FindPath(start, target, 6);

        // Assert
        path.Should().NotBeNull();
        path!.Should().Contain(new HexCoordinates(1, 2)); // Should go through clear terrain
        path!.Should().Contain(new HexCoordinates(1, 3)); // Should go through clear terrain
    }

    [Fact]
    public void GetReachableHexes_WithClearTerrain_ReturnsCorrectHexes()
    {
        // Arrange
        var map = new BattleMap(5, 5);
        var start = new HexCoordinates(3, 3);

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
        var map = new BattleMap(2, 2);
        var start = new HexCoordinates(1, 1);

        // Add clear terrain hex
        var clearHex = new Hex(new HexCoordinates(2, 1));
        clearHex.AddTerrain(new ClearTerrain());
        map.AddHex(clearHex);

        // Add heavy woods hex
        var woodsHex = new Hex(new HexCoordinates(1, 2));
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
        var map = new BattleMap(4, 1);
        var start = new HexCoordinates(1, 1);
        var end = new HexCoordinates(4, 1);

        // Add clear terrain hexes
        for (var q = 1; q <= 4; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 1));
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
        var map = new BattleMap(4, 1);
        var start = new HexCoordinates(1, 1);
        var end = new HexCoordinates(4, 1);

        // Add clear terrain hexes
        for (var q = 1; q <= 4; q++)
        {
            var hex = new Hex(new HexCoordinates(q, 1));
            hex.AddTerrain(new ClearTerrain());
            map.AddHex(hex);
        }

        // Add blocking heavy woods in the middle
        var blockingHex = new Hex(new HexCoordinates(2, 1), 2); // Higher base level
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
        var generator = Substitute.For<ITerrainGenerator>();
        generator.Generate(Arg.Any<HexCoordinates>())
            .Returns(c => new Hex(c.Arg<HexCoordinates>()));

        // Act
        var map = BattleMap.GenerateMap(width, height, generator);

        // Assert
        map.Width.Should().Be(width);
        map.Height.Should().Be(height);

        // Check if all hexes are created
        for (var r = 1; r < height+1; r++)
        {
            for (var q = 1; q < width+1; q++)
            {
                var hex = map.GetHex(new HexCoordinates(q, r));
                hex.Should().NotBeNull();
            }
        }

        // Verify generator was called for each hex
        generator.Received(width * height).Generate(Arg.Any<HexCoordinates>());
    }

    [Fact]
    public void GenerateMap_WithTerrainGenerator_CreatesCorrectTerrain()
    {
        // Arrange
        const int width = 3;
        const int height = 3;
        var generator = Substitute.For<ITerrainGenerator>();
        generator.Generate(Arg.Any<HexCoordinates>())
            .Returns(c => {
                var hex = new Hex(c.Arg<HexCoordinates>());
                hex.AddTerrain(new ClearTerrain());
                return hex;
            });

        // Act
        var map = BattleMap.GenerateMap(width, height, generator);

        // Assert
        // Check all hexes have clear terrain
        for (var q = 1; q < width+1; q++)
        {
            for (var r = 1; r < height+1; r++)
            {
                var hex = map.GetHex(new HexCoordinates(q, r));
                hex!.HasTerrain("Clear").Should().BeTrue();
            }
        }

        // Verify generator was called for each hex
        generator.Received(width * height).Generate(Arg.Any<HexCoordinates>());
    }
    
    [Fact]
    public void CreateFromData_ShouldCloneHexesCorrectly()
    {
        // Arrange
        var originalMap = new BattleMap(2, 2);
        var woodHex = new Hex(new HexCoordinates(1, 1), 1);
        woodHex.AddTerrain(new HeavyWoodsTerrain());
        originalMap.AddHex(woodHex);
        originalMap.AddHex(new Hex(new HexCoordinates(1, 1),2));
        originalMap.AddHex(new Hex(new HexCoordinates(1, 2)));
        originalMap.AddHex(new Hex(new HexCoordinates(2, 1)));
        
        var hexDataList = originalMap.GetHexes().Select(hex => hex.ToData()).ToList();

        // Act
        var clonedMap = BattleMap.CreateFromData(hexDataList);

        // Assert
        foreach (var hex in originalMap.GetHexes())
        {
            var clonedHex = clonedMap.GetHex(hex.Coordinates);
            clonedHex.Should().NotBeNull();
            clonedHex.Level.Should().Be(hex.Level);
            clonedHex.GetTerrainTypes().Should().BeEquivalentTo(hex.GetTerrainTypes());
        }
    }
}
