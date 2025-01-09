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
    /// Finds the shortest path between hexes considering movement costs and facing changes
    /// </summary>
    public List<HexPosition>? FindPath(
        HexPosition start,
        HexPosition target,
        int maxMovementPoints)
    {
        var frontier = new PriorityQueue<(HexPosition pos, int cost), int>();
        frontier.Enqueue((start, 0), 0);
        
        var cameFrom = new Dictionary<HexPosition, HexPosition>();
        var costSoFar = new Dictionary<HexPosition, int>
        {
            [start] = 0
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue().pos;
            
            if (current.Coordinates == target.Coordinates)
                break;

            // For each adjacent hex
            foreach (var nextCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map
                var nextHex = GetHex(nextCoord);
                if (nextHex == null)
                    continue;

                // For each possible facing in the next hex
                for (int facing = 0; facing < 6; facing++)
                {
                    var next = new HexPosition(nextCoord, (HexDirection)facing);
                    
                    // Calculate movement cost including turning
                    var turningCost = current.GetTurningCost(next.Facing);
                    var newCost = costSoFar[current] + nextHex.MovementCost + turningCost;
                    
                    if (newCost > maxMovementPoints) // Exceeds movement points
                        continue;

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + next.Coordinates.DistanceTo(target.Coordinates); // A* heuristic
                        frontier.Enqueue((next, newCost), priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        // Find the best final position with desired facing
        var finalPositions = cameFrom.Keys.Where(p => p.Coordinates == target.Coordinates);
        var bestFinalPos = finalPositions.MinBy(p => costSoFar[p] + p.GetTurningCost(target.Facing));
        
        if (bestFinalPos == null)
            return null;

        // Reconstruct path
        var path = new List<HexPosition>();
        var currentPosition = bestFinalPos;
        while (currentPosition.Coordinates != start.Coordinates)
        {
            path.Add(currentPosition);
            currentPosition = cameFrom[currentPosition];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Gets all valid hexes that can be reached with given movement points, considering facing
    /// </summary>
    public IEnumerable<(HexPosition position, int cost)> GetReachableHexes(
        HexPosition start,
        int maxMovementPoints)
    {
        var visited = new Dictionary<HexPosition, int>();
        var toVisit = new Queue<HexPosition>();
        
        visited[start] = 0;
        toVisit.Enqueue(start);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            var currentCost = visited[current];

            foreach (var neighborCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map
                var neighborHex = GetHex(neighborCoord);
                if (neighborHex == null)
                    continue;

                // Try all possible facings
                for (int facing = 0; facing < 6; facing++)
                {
                    var neighborPos = new HexPosition(neighborCoord, (HexDirection)facing);
                    
                    // Calculate total cost including turning
                    var turningCost = current.GetTurningCost(neighborPos.Facing);
                    var totalCost = currentCost + neighborHex.MovementCost + turningCost;
                    
                    if (totalCost > maxMovementPoints) // Exceeds movement points
                        continue;

                    if (visited.TryAdd(neighborPos, totalCost))
                    {
                        toVisit.Enqueue(neighborPos);
                    }
                    else if (totalCost < visited[neighborPos])
                    {
                        visited[neighborPos] = totalCost;
                        toVisit.Enqueue(neighborPos);
                    }
                }
            }
        }

        return visited.Select(v => (v.Key, v.Value)).Where(x => x.Key != start);
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
