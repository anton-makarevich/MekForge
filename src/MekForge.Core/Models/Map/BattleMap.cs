using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Utils.Generators;

namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents the game battle map, managing hexes and providing pathfinding capabilities
/// </summary>
public class BattleMap
{
    private readonly Dictionary<HexCoordinates, Hex> _hexes = new();

    public int Width { get; }
    public int Height { get; }

    public BattleMap(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Adds a hex to the map. Throws HexOutsideOfMapBoundariesException if hex coordinates are outside map boundaries
    /// </summary>
    public void AddHex(Hex hex)
    {
        if (hex.Coordinates.Q < 1 || hex.Coordinates.Q >= Width +1 ||
            hex.Coordinates.R < 1 || hex.Coordinates.R >= Height +1)
        {
            throw new HexOutsideOfMapBoundariesException(hex.Coordinates, Width, Height);
        }
        
        _hexes[hex.Coordinates] = hex;
    }

    public Hex? GetHex(HexCoordinates coordinates)
    {
        return _hexes.GetValueOrDefault(coordinates);
    }

    /// <summary>
    /// Finds a path between two positions, considering facing direction and movement costs
    /// </summary>
    public List<HexPosition>? FindPath(HexPosition start, HexPosition target, int maxMovementPoints)
    {
        var frontier = new PriorityQueue<(HexPosition pos, List<HexPosition> path), int>();
        var visited = new HashSet<(HexCoordinates coords, HexDirection facing)>();
        
        frontier.Enqueue((start, [start]), 0);
        visited.Add((start.Coordinates, start.Facing));

        while (frontier.Count > 0)
        {
            var (current, path) = frontier.Dequeue();
            
            // Check if we've reached the target
            if (current.Coordinates == target.Coordinates && current.Facing == target.Facing)
            {
                return path;
            }

            // For each adjacent hex
            foreach (var nextCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                var hex = GetHex(nextCoord);
                if (hex == null)
                    continue;

                // Get required facing for movement
                var requiredFacing = current.Coordinates.GetDirectionToNeighbour(nextCoord);
                
                // Calculate turning steps if needed
                var turningSteps = current.GetTurningSteps(requiredFacing).ToList();
                var newPath = new List<HexPosition>(path);
                newPath.AddRange(turningSteps);
                
                // Add the movement step
                var nextPos = new HexPosition(nextCoord, requiredFacing);
                newPath.Add(nextPos);
                
                // Calculate total cost
                var totalSteps = newPath.Count - 1; // -1 because start position doesn't count as a step
                
                if (totalSteps > maxMovementPoints)
                    continue;
                    
                // Skip if we've already visited this state
                if (!visited.Add((nextCoord, requiredFacing)))
                    continue;
                
                // Calculate priority based on remaining distance plus current cost
                var priority = totalSteps + nextCoord.DistanceTo(target.Coordinates);
                
                // If we're at target coordinates but wrong facing, add turning steps to target facing
                if (nextCoord == target.Coordinates && requiredFacing != target.Facing)
                {
                    var finalTurningSteps = nextPos.GetTurningSteps(target.Facing).ToList();
                    if (totalSteps + finalTurningSteps.Count > maxMovementPoints) continue;
                    newPath.AddRange(finalTurningSteps);
                    frontier.Enqueue((new HexPosition(nextCoord, target.Facing), newPath), priority);
                }
                else
                {
                    frontier.Enqueue((nextPos, newPath), priority);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all valid hexes that can be reached with given movement points, considering facing
    /// </summary>
    public IEnumerable<(HexCoordinates coordinates, int cost)> GetReachableHexes(
        HexPosition start,
        int maxMovementPoints,
        IEnumerable<HexCoordinates>? prohibitedHexes = null)
    {
        var visited = new Dictionary<(HexCoordinates coords, HexDirection facing), int>();
        var bestCosts = new Dictionary<HexCoordinates, int>();
        var toVisit = new Queue<HexPosition>();
        var prohibited = prohibitedHexes?.ToHashSet() ?? [];
        
        visited[(start.Coordinates, start.Facing)] = 0;
        bestCosts[start.Coordinates] = 0;
        toVisit.Enqueue(start);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            var currentCost = visited[(current.Coordinates, current.Facing)];

            // For each adjacent hex
            foreach (var neighborCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map or is prohibited
                var neighborHex = GetHex(neighborCoord);
                if (neighborHex == null || prohibited.Contains(neighborCoord))
                    continue;

                // Get required facing to move to this hex
                var requiredFacing = current.Coordinates.GetDirectionToNeighbour(neighborCoord);
                
                // Calculate turning cost from current facing
                var turningCost = current.GetTurningCost(requiredFacing);
                
                // Calculate total cost including turning and movement
                var totalCost = currentCost + neighborHex.MovementCost + turningCost;
                
                if (totalCost > maxMovementPoints) // Exceeds movement points
                    continue;

                var neighborKey = (neighborCoord, requiredFacing);
                
                // Skip if we've visited this hex+facing combination with a lower cost
                if (visited.TryGetValue(neighborKey, out var visitedCost) && totalCost >= visitedCost)
                    continue;
                
                // Update both visited and best costs
                visited[neighborKey] = totalCost;
                if (!bestCosts.TryGetValue(neighborCoord, out var bestCost) || totalCost < bestCost)
                {
                    bestCosts[neighborCoord] = totalCost;
                }
                
                toVisit.Enqueue(new HexPosition(neighborCoord, requiredFacing));
            }
        }

        return bestCosts
            .Where(v => v.Key != start.Coordinates)
            .Select(v => (v.Key, v.Value));
    }

    /// <summary>
    /// Checks if there is line of sight between two hexes
    /// </summary>
    public bool HasLineOfSight(HexCoordinates from, HexCoordinates to)
    {
        var fromHex = GetHex(from);
        var toHex = GetHex(to);
        if (fromHex == null || toHex == null)
            return false;

        // Get hexes along the line
        var hexLine = from.LineTo(to);
        var distance = 1;
        var totalDistance = from.DistanceTo(to);

        foreach (var coordinates in hexLine.Skip(1)) // Skip the starting hex
        {
            var hex = GetHex(coordinates);
            if (hex == null)
                return false;

            // Calculate the minimum height needed at this distance to maintain LOS
            var requiredHeight = InterpolateHeight(
                fromHex.GetCeiling(),
                toHex.GetCeiling(),
                distance,
                totalDistance);

            // If the hex is higher than the line between start and end points, it blocks LOS
            if (hex.GetCeiling() > requiredHeight)
                return false;

            distance++;
        }

        return true;
    }
    

    /// <summary>
    /// Interpolate height between two points for LOS calculation
    /// </summary>
    private static int InterpolateHeight(int startHeight, int endHeight, int currentDistance, int totalDistance)
    {
        if (totalDistance == 0)
            return startHeight;

        var t = (double)currentDistance / totalDistance;
        return (int)Math.Round(startHeight + (endHeight - startHeight) * t);
    }

    /// <summary>
    /// Generate a rectangular map with the specified terrain generator
    /// </summary>
    public static BattleMap GenerateMap(int width, int height, ITerrainGenerator generator)
    {
        var map = new BattleMap(width, height);

        for (var q = 1; q < width+1; q++)
        {
            for (var r = 1; r < height+1; r++)
            {
                var coordinates = new HexCoordinates(q, r);
                var hex = generator.Generate(coordinates);
                map._hexes[coordinates] = hex;
            }
        }

        return map;
    }

    public static BattleMap CreateFromData(IList<HexData> hexData)
    {
        var map = new BattleMap(
            hexData.Max(h => h.Coordinates.Q) + 1,
            hexData.Max(h => h.Coordinates.R) + 1);
        foreach (var hex in hexData)
        {
            var newHex = new Hex(new HexCoordinates(hex.Coordinates), hex.Level);
            foreach (var terrainType in hex.TerrainTypes)
            {
                // Map terrain type strings to terrain classes
                var terrain = Terrain.GetTerrainType(terrainType);
                newHex.AddTerrain(terrain);
            }
            map.AddHex(newHex);
        }
        return map;
    }

    public IEnumerable<Hex> GetHexes()
    {
        return _hexes.Values;
    }
}
