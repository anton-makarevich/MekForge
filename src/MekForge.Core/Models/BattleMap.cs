using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Utils.Generators;

namespace Sanet.MekForge.Core.Models;

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
        if (hex.Coordinates.Q < 0 || hex.Coordinates.Q >= Width ||
            hex.Coordinates.R < 0 || hex.Coordinates.R >= Height)
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
    /// Finds the shortest path between hexes considering movement costs
    /// </summary>
    public List<HexCoordinates>? FindPath(
        HexCoordinates start,
        HexCoordinates target,
        int maxMovementPoints)
    {
        var frontier = new PriorityQueue<(HexCoordinates pos, int cost), int>();
        frontier.Enqueue((start, 0), 0);
        
        var cameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
        var costSoFar = new Dictionary<HexCoordinates, int>
        {
            [start] = 0
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue().pos;
            
            if (current == target)
                break;

            foreach (var next in current.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map
                var nextHex = GetHex(next);
                if (nextHex == null)
                    continue;

                var newCost = costSoFar[current] + nextHex.MovementCost;
                if (newCost > maxMovementPoints) // Exceeds movement points
                    continue;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + next.DistanceTo(target); // A* heuristic
                    frontier.Enqueue((next, newCost), priority);
                    cameFrom[next] = current;
                }
            }
        }

        // If we didn't reach the target
        if (!cameFrom.ContainsKey(target))
            return null;

        // Reconstruct path
        var path = new List<HexCoordinates>();
        var currentCoordinates = target;
        while (currentCoordinates != start)
        {
            path.Add(currentCoordinates);
            currentCoordinates = cameFrom[currentCoordinates];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Gets all valid hexes that can be reached with given movement points
    /// </summary>
    public IEnumerable<(HexCoordinates coordinates, int cost)> GetReachableHexes(
        HexCoordinates start,
        int maxMovementPoints)
    {
        var visited = new Dictionary<HexCoordinates, int>();
        var toVisit = new Queue<HexCoordinates>();
        
        visited[start] = 0;
        toVisit.Enqueue(start);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            var currentCost = visited[current];

            foreach (var neighbor in current.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map
                var neighborHex = GetHex(neighbor);
                if (neighborHex == null)
                    continue;

                var totalCost = currentCost + neighborHex.MovementCost;
                if (totalCost > maxMovementPoints) // Exceeds movement points
                    continue;

                if (visited.TryAdd(neighbor, totalCost))
                {
                    toVisit.Enqueue(neighbor);
                }
                else if (totalCost < visited[neighbor])
                {
                    visited[neighbor] = totalCost;
                    toVisit.Enqueue(neighbor);
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
        var hexLine = GetHexesAlongLine(from, to);
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
    /// Gets coordinates of hexes that form a line between two points
    /// </summary>
    private static IEnumerable<HexCoordinates> GetHexesAlongLine(HexCoordinates from, HexCoordinates to)
    {
        var distance = from.DistanceTo(to);
        if (distance == 0)
            return [from];

        var results = new List<HexCoordinates> { from };
        
        for (var i = 1; i <= distance; i++)
        {
            var t = (double)i / distance;
            var qLerp = from.Q + (to.Q - from.Q) * t;
            var rLerp = from.R + (to.R - from.R) * t;
            
            // Round to nearest hex
            var q = (int)Math.Round(qLerp);
            var r = (int)Math.Round(rLerp);
            
            results.Add(new HexCoordinates(q, r));
        }

        return results;
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

        for (var q = 0; q < width; q++)
        {
            for (var r = 0; r < height; r++)
            {
                var coordinates = new HexCoordinates(q, r);
                var hex = generator.Generate(coordinates);
                map._hexes[coordinates] = hex;
            }
        }

        return map;
    }

    public IEnumerable<Hex> GetHexes()
    {
        return _hexes.Values;
    }
}
