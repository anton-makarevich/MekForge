using Shouldly;
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
        map.Width.ShouldBe(width);
        map.Height.ShouldBe(height);
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
        map.GetHex(hex.Coordinates).ShouldBe(hex);
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
        var ex =Should.Throw<HexOutsideOfMapBoundariesException>(()=>action);
        ex.Coordinates.ShouldBe(hex.Coordinates);
        ex.MapWidth.ShouldBe(2); 
        ex.MapHeight.ShouldBe(2);
    }

    [Fact]
    public void FindPath_WithFacingChanges_ConsidersTurningCost()
    {
        // Arrange
        var map = new BattleMap(3, 1);
        var start = new HexPosition(new HexCoordinates(1, 1), HexDirection.Top);
        var target = new HexPosition(new HexCoordinates(3, 1), HexDirection.Bottom);

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
        path.ShouldNotBeNull();
        path.Count.ShouldBe(7); // Should include direction changes
        path.Select(p => (p.To.Coordinates, p.To.Facing)).ShouldBe([
            (new HexCoordinates(1, 1), HexDirection.TopRight),
            (new HexCoordinates(1, 1), HexDirection.BottomRight),
            (new HexCoordinates(2, 1), HexDirection.BottomRight),
            (new HexCoordinates(2, 1), HexDirection.TopRight),
            (new HexCoordinates(3, 1), HexDirection.TopRight),
            (new HexCoordinates(3, 1), HexDirection.BottomRight),
            (new HexCoordinates(3, 1), HexDirection.Bottom)
        ]);
    }

    [Fact]
    public void GetReachableHexes_WithClearTerrain_ReturnsCorrectHexes()
    {
        // Arrange
        var map = BattleMap.GenerateMap(5, 5,
            new SingleTerrainGenerator(5, 5, new ClearTerrain()));
        var start = new HexPosition(new HexCoordinates(3, 3), HexDirection.Top);

        // Act
        var reachable = map.GetReachableHexes(start, 2).ToList();

        // Assert
        reachable.Count.ShouldBe(4); // 
        reachable.All(h => h.cost <= 2).ShouldBeTrue();
        reachable.Count(h => h.cost == 1).ShouldBe(1); // 6 adjacent hexes
        reachable.Count(h => h.cost == 2).ShouldBe(3); // 12 hexes at distance 2
    }

    [Fact]
    public void GetReachableHexes_WithMixedTerrain_ConsidersTerrainCosts()
    {
        // Arrange
        var map = new BattleMap(2, 2);
        var start = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);

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
        reachable.Count.ShouldBe(1); // Only the clear hex should be reachable
        reachable.First().coordinates.ShouldBe(clearHex.Coordinates);
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
        hasLos.ShouldBeTrue();
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
        hasLos.ShouldBeFalse();
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
        map.Width.ShouldBe(width);
        map.Height.ShouldBe(height);

        // Check if all hexes are created
        for (var r = 1; r < height+1; r++)
        {
            for (var q = 1; q < width+1; q++)
            {
                var hex = map.GetHex(new HexCoordinates(q, r));
                hex.ShouldNotBeNull();
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
                hex!.HasTerrain("Clear").ShouldBeTrue();
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
            clonedHex.ShouldNotBeNull();
            clonedHex.Level.ShouldBe(hex.Level);
            clonedHex.GetTerrainTypes().ShouldBeEquivalentTo(hex.GetTerrainTypes());
        }
    }

    [Fact]
    public void GetReachableHexes_WithComplexTerrainAndFacing_ReachesHexThroughClearPath()
    {
        // Arrange
        var map = BattleMap.GenerateMap(11, 9,
            new SingleTerrainGenerator(11, 9, new ClearTerrain())); // Size to fit all hexes (0-10, 0-8)

        // Heavy Woods
        var heavyWoodsCoords = new[]
        {
            (4, 7), (5, 7), (6, 8),
            (8, 3), (9, 3), (8, 4), (9, 4),
            (9, 8), (10, 6)
        };
        foreach (var (q, r) in heavyWoodsCoords)
        {
            var hex = map.GetHex(new HexCoordinates(q, r));
            hex!.RemoveTerrain("Clear");
            hex.AddTerrain(new HeavyWoodsTerrain());
        }

        // Light Woods
        var lightWoodsCoords = new[]
        {
            (4, 6), (9, 6), (9, 7), (10, 7)
        };
        foreach (var (q, r) in lightWoodsCoords)
        {
            var hex = map.GetHex(new HexCoordinates(q, r));
            hex!.RemoveTerrain("Clear");
            hex.AddTerrain(new LightWoodsTerrain());
        }

        // Starting position: 9,5 facing bottom-left (direction 4)
        var start = new HexPosition(new HexCoordinates(9, 5), HexDirection.BottomLeft);
        const int maxMp = 5;

        // Act
        var reachableHexes = map.GetReachableHexes(start, maxMp).ToList();

        // Assert
        var targetHex = new HexCoordinates(7, 8);
        reachableHexes.ShouldContain(x => x.coordinates == targetHex,
            "Hex (7,8) should be reachable through path: (9,5)->(8,5)->(7,6)->[turn]->(7,7)->(7,8)");

        // Verify the path exists and respects movement points
        var path = map.FindPath(
            start,
            new HexPosition(targetHex, HexDirection.Bottom),
            maxMp);

        path.ShouldNotBeNull("A valid path should exist to reach (7,8)");

        path.Count.ShouldBeLessThanOrEqualTo(maxMp + 1,
            "Path length should not exceed maxMP + 1 (including start position)");

        var pathCoords = path.Select(p => p.To.Coordinates).Distinct().ToList();
        pathCoords.ShouldContain(new HexCoordinates(8, 5), "Path should go through (8,5)");
        pathCoords.ShouldContain(new HexCoordinates(7, 6), "Path should go through (7,6)");
        pathCoords.ShouldContain(new HexCoordinates(7, 7), "Path should go through (7,7)");
        pathCoords.ShouldContain(targetHex, "Path should reach (7,8)");
    }

    [Fact]
    public void GetReachableHexes_WithProhibitedHexes_ExcludesProhibitedHexes()
    {
        // Arrange
        var map = BattleMap.GenerateMap(3, 3,
            new SingleTerrainGenerator(3,3, new ClearTerrain()));
        var start = new HexPosition(new HexCoordinates(2, 2), HexDirection.Top);

        // Create prohibited hexes - block two adjacent hexes
        var prohibitedHexes = new List<HexCoordinates>
        {
            new(2, 1), // Hex above start
            new(3, 2)  // Hex to the right of start
        };

        // Act
        var reachable = map.GetReachableHexes(start, 2, prohibitedHexes).ToList();

        // Assert
        reachable.ShouldNotBeEmpty("Some hexes should be reachable");
        reachable.ShouldNotContain(h => prohibitedHexes.Contains(h.coordinates), 
            "Prohibited hexes should not be included in reachable hexes");
    }

    [Fact]
    public void FindPath_WithProhibitedHexes_FindsAlternativePath()
    {
        // Arrange
        var map = BattleMap.GenerateMap(3, 3,
            new SingleTerrainGenerator(3,3, new ClearTerrain()));
        var start = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var target = new HexPosition(new HexCoordinates(3, 3), HexDirection.Bottom);
        
        // Create prohibited hexes that block the direct path
        var prohibitedHexes = new[]
        {
            new HexCoordinates(2, 2),
            new HexCoordinates(3, 2)
        };

        // Act
        var path = map.FindPath(start, target, 10, prohibitedHexes);

        // Assert
        path.ShouldNotBeNull();
        var pathCoordinates = path.Select(p => p.To.Coordinates).ToList();
        foreach (var prohibitedHex in prohibitedHexes)
        {
            pathCoordinates.ShouldNotContain(prohibitedHex);
        }
        pathCoordinates.ShouldContain(new HexCoordinates(1, 2)); // Should go around through the left side
        pathCoordinates.ShouldContain(new HexCoordinates(2, 3));
    }

    [Fact]
    public void FindPath_WithTerrainCosts_ShouldConsiderMovementCosts()
    {
        // Arrange
        var map = BattleMap.GenerateMap(2, 5, 
            new SingleTerrainGenerator(2,5, new ClearTerrain()));
        var start = new HexPosition(new HexCoordinates(1, 1), HexDirection.Bottom);
        var target = new HexPosition(new HexCoordinates(1, 5), HexDirection.Bottom);

        // Add two possible paths:
        // Path 1 (direct but costly): Through heavy woods (1,1)->(1,2)->(1,3)->(1,4)->(1,5)
        //   Cost: 3+3+3+1 = 10 MP (each heavy woods hex costs 3)
        // Path 2 (longer but cheaper): Around through clear terrain (1,1)->(2,1)->(2,2)->(2,3)->(2,4)->(1,5)
        //   Cost: 1+1+1+1+1 = 5 MP (clear terrain) + 4 MP (direction changes) = 9 MP total

        // Add heavy woods on the direct path
        var woodsHexes = new[]
        {
            new HexCoordinates(1, 2),
            new HexCoordinates(1, 3),
            new HexCoordinates(1, 4)
        };

        foreach (var coord in woodsHexes)
        {
            var hex = new Hex(coord);
            hex.AddTerrain(new HeavyWoodsTerrain()); // Movement cost 3
            map.AddHex(hex);
        }

        // Act
        var path = map.FindPath(start, target, 9);

        // Assert
        path.ShouldNotBeNull("A path should exist within 9 movement points");
        
        // The path should go through clear terrain to avoid heavy woods
        var pathCoords = path.Select(p => p.To.Coordinates).Distinct().ToList();
        pathCoords.ShouldContain(new HexCoordinates(2, 1), "Path should go through clear terrain at (2,1)");
        pathCoords.ShouldContain(new HexCoordinates(2, 2), "Path should go through clear terrain at (2,2)");
        pathCoords.ShouldContain(new HexCoordinates(2, 3), "Path should go through clear terrain at (2,3)");
        pathCoords.ShouldContain(new HexCoordinates(2, 4), "Path should go through clear terrain at (2,4)");
        woodsHexes.ShouldNotContain(coord => pathCoords.Contains(coord), 
            "Path should avoid all heavy woods hexes");

        // Verify path costs
        var totalCost = path.Sum(s => s.Cost);
        totalCost.ShouldBe(9, "Total path cost should be 9 MP (5 MP for movement + 4 MP for turns)");
        
        // Verify movement costs
        var movementSegments = path.Where(s => s.From.Coordinates != s.To.Coordinates).ToList();
        movementSegments.ShouldAllBe(s => s.Cost == 1, "All movement segments should cost 1 MP as they go through clear terrain");
        
        // Verify turning costs
        var turnSegments = path.Where(s => s.From.Coordinates == s.To.Coordinates).ToList();
        turnSegments.Count.ShouldBe(4, "Should have 4 turns");
        turnSegments.ShouldAllBe(s => s.Cost==1, 
            "All turn segments should cost 1 MP");
    }

    [Theory]
    [InlineData(typeof(ClearTerrain))] 
    [InlineData(typeof(LightWoodsTerrain))] 
    [InlineData(typeof(HeavyWoodsTerrain))] 
    public void GetJumpReachableHexes_WithDifferentTerrains_IgnoresTerrainCost(Type terrainType)
    {
        // Arrange
        var terrain = (Terrain)Activator.CreateInstance(terrainType)!;
        var map = BattleMap.GenerateMap(5, 5,
            new SingleTerrainGenerator(5, 5, terrain));
        var start = new HexCoordinates(3, 3);
        const int movementPoints = 2; 

        // Act
        var reachableHexes = map.GetJumpReachableHexes(start, movementPoints).ToList();

        // Assert
        reachableHexes.Count.ShouldBe(18, 
            $"Should have 18 total reachable hexes with {terrainType.Name}");
        reachableHexes.ShouldNotContain(start, 
            "Should not include start hex");
        reachableHexes.All(h => h.DistanceTo(start) <= movementPoints).ShouldBeTrue(
            "All hexes should be within movement range");
        
        // Verify we have correct number of hexes at each distance
        reachableHexes.Count(h => h.DistanceTo(start) == 1).ShouldBe(6, 
            "Should have 6 hexes at distance 1");
        reachableHexes.Count(h => h.DistanceTo(start) == 2).ShouldBe(12, 
            "Should have 12 hexes at distance 2");
    }

    [Fact]
    public void GetJumpReachableHexes_WithProhibitedHexes_ExcludesProhibitedHexes()
    {
        // Arrange
        var map = BattleMap.GenerateMap(5, 5,
            new SingleTerrainGenerator(5, 5, new ClearTerrain()));
        var start = new HexCoordinates(3, 3);
        const int movementPoints = 2;

        // Prohibit some adjacent hexes
        var prohibitedHexes = start.GetAdjacentCoordinates().Take(3).ToList();

        // Act
        var reachableHexes = map.GetJumpReachableHexes(start, movementPoints, prohibitedHexes).ToList();

        // Assert
        prohibitedHexes.ForEach(coordinates => reachableHexes.ShouldNotContain(coordinates));
        reachableHexes.All(h => h.DistanceTo(start) <= movementPoints).ShouldBeTrue();
    }

    [Fact]
    public void GetJumpReachableHexes_AtMapEdge_ReturnsOnlyValidHexes()
    {
        // Arrange
        var map = BattleMap.GenerateMap(3, 3,
            new SingleTerrainGenerator(3, 3, new ClearTerrain()));
        var start = new HexCoordinates(1, 1); // Corner hex
        const int movementPoints = 2;

        // Act
        var reachableHexes = map.GetJumpReachableHexes(start, movementPoints).ToList();

        // Assert
        reachableHexes.ShouldAllBe(h => 
                h.Q >= 1 && h.Q <= 3 && 
                h.R >= 1 && h.R <= 3, 
            "All hexes should be within map boundaries (Q: 1-3, R: 1-3)");

        reachableHexes.ShouldAllBe(h => 
                h.DistanceTo(start) <= movementPoints,
            $"All hexes should be within {movementPoints} movement points from start");
    }
}
