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
    public List<PathSegment>? FindPath(HexPosition start, HexPosition target, int maxMovementPoints, IEnumerable<HexCoordinates>? prohibitedHexes = null)
    {
        var frontier = new PriorityQueue<(HexPosition pos, List<HexPosition> path, int cost), int>();
        var visited = new Dictionary<(HexCoordinates coords, HexDirection facing), int>();
        var prohibited = prohibitedHexes?.ToHashSet() ?? new HashSet<HexCoordinates>();
        
        frontier.Enqueue((start, [start], 0), 0);
        visited[(start.Coordinates, start.Facing)] = 0;

        while (frontier.Count > 0)
        {
            var (current, path, currentCost) = frontier.Dequeue();
            
            // Check if we've reached the target
            if (current.Coordinates == target.Coordinates && current.Facing == target.Facing)
            {
                // Convert path to segments
                var segments = new List<PathSegment>();
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var from = path[i];
                    var to = path[i + 1];
                    var segmentCost = 1; // Default cost for turning

                    // If coordinates changed, it's a movement
                    if (from.Coordinates != to.Coordinates)
                    {
                        var hex = GetHex(to.Coordinates);
                        segmentCost = hex!.MovementCost;
                    }

                    segments.Add(new PathSegment(from, to, segmentCost));
                }
                return segments;
            }

            // For each adjacent hex
            foreach (var nextCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                var hex = GetHex(nextCoord);
                if (hex == null || prohibited.Contains(nextCoord))
                    continue;

                // Get required facing for movement
                var requiredFacing = current.Coordinates.GetDirectionToNeighbour(nextCoord);
                
                // Calculate turning steps and cost if needed
                var turningSteps = current.GetTurningSteps(requiredFacing).ToList();
                var turningCost = turningSteps.Count;
                var newPath = new List<HexPosition>(path);
                newPath.AddRange(turningSteps);
                
                // Add the movement step
                var nextPos = new HexPosition(nextCoord, requiredFacing);
                newPath.Add(nextPos);
                
                // Calculate total cost including terrain
                var totalCost = currentCost + hex.MovementCost + turningCost;
                
                if (totalCost > maxMovementPoints)
                    continue;
                    
                // Skip if we've visited this state with a lower or equal cost
                var nextKey = (nextCoord, requiredFacing);
                if (visited.TryGetValue(nextKey, out var visitedCost) && totalCost >= visitedCost)
                    continue;
                
                visited[nextKey] = totalCost;
                
                // Calculate priority based on remaining distance plus current cost
                var priority = totalCost + nextCoord.DistanceTo(target.Coordinates);
                
                // If we're at target coordinates but wrong facing, add turning steps to target facing
                if (nextCoord == target.Coordinates && requiredFacing != target.Facing)
                {
                    var finalTurningSteps = nextPos.GetTurningSteps(target.Facing).ToList();
                    var finalCost = totalCost + finalTurningSteps.Count;
                    if (finalCost > maxMovementPoints) continue;
                    newPath.AddRange(finalTurningSteps);
                    frontier.Enqueue((new HexPosition(nextCoord, target.Facing), newPath, finalCost), priority);
                }
                else
                {
                    frontier.Enqueue((nextPos, newPath, totalCost), priority);
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
    /// Gets all valid hexes that can be reached with jumping movement, where each hex costs 1 MP
    /// regardless of terrain or facing direction
    /// </summary>
    public IEnumerable<HexCoordinates> GetJumpReachableHexes(
        HexCoordinates start,
        int movementPoints,
        IEnumerable<HexCoordinates>? prohibitedHexes = null)
    {
        var prohibited = prohibitedHexes?.ToHashSet() ?? [];
        
        // Get all hexes within range using the existing method
        return start.GetCoordinatesInRange(movementPoints)
            .Where(coordinates =>
            {
                // Skip if hex doesn't exist on map or is prohibited
                var hex = GetHex(coordinates);
                return hex != null && 
                       !prohibited.Contains(coordinates) &&
                       coordinates != start;
            });
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

    public List<PathSegment>? FindJumpPath(HexPosition from, HexPosition to, int movementPoints)
    {
        if (!IsValidPosition(from.Coordinates) || !IsValidPosition(to.Coordinates))
            return null;

        var distance = from.Coordinates.DistanceTo(to.Coordinates);
        if (distance > movementPoints)
            return null;

        // For jumping, we want the shortest path ignoring terrain and turning costs
        var path = new List<PathSegment>();
        var currentPosition = from;
        var remainingDistance = distance;

        while (remainingDistance > 0)
        {
            // Find the next hex in the direction of the target
            var neighbors = currentPosition.Coordinates.GetAdjacentCoordinates()
                .Where(IsValidPosition)
                .ToList();

            // Get the neighbor that's closest to the target
            var nextCoords = neighbors
                .OrderBy(n => n.DistanceTo(to.Coordinates))
                .First();

            // Add path segment with cost 1 (each hex costs 1 MP for jumping)
            var nextPosition = (to.Coordinates==nextCoords)
                ? to 
                : new HexPosition(nextCoords, currentPosition.Coordinates.GetDirectionToNeighbour(nextCoords));
            
            path.Add(new PathSegment(
                currentPosition,
                nextPosition,
                1));

            currentPosition = nextPosition;
            remainingDistance--;
        }

        return path;
    }

    private bool IsValidPosition(HexCoordinates coordinates)
    {
        return coordinates.Q >= 1 && coordinates.Q <= Width &&
               coordinates.R >= 1 && coordinates.R <= Height;
    }
}
