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
        // First, find the shortest path without considering facing
        var frontier = new PriorityQueue<(HexCoordinates coords, int cost), int>();
        frontier.Enqueue((start.Coordinates, 0), 0);
        
        var cameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
        var costSoFar = new Dictionary<HexCoordinates, int>
        {
            [start.Coordinates] = 0
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue().coords;

            if (current == target.Coordinates)
                break;

            foreach (var next in current.GetAdjacentCoordinates())
            {
                var nextHex = GetHex(next);
                if (nextHex == null)
                    continue;

                var newCost = costSoFar[current] + nextHex.MovementCost;
                if (newCost > maxMovementPoints)
                    continue;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    var priority = newCost + next.DistanceTo(target.Coordinates);
                    frontier.Enqueue((next, newCost), priority);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(target.Coordinates))
            return null;

        // Reconstruct the path of coordinates
        var coordPath = new List<HexCoordinates>();
        var currentCoord = target.Coordinates;
        while (currentCoord != start.Coordinates)
        {
            coordPath.Add(currentCoord);
            currentCoord = cameFrom[currentCoord];
        }
        coordPath.Add(start.Coordinates);
        coordPath.Reverse();

        // Now create the final path with facing changes
        var result = new List<HexPosition>();
        var currentFacing = start.Facing;
        
        // Add start position
        result.Add(start);

        // For each coordinate except the last one
        for (int i = 0; i < coordPath.Count - 1; i++)
        {
            var current = coordPath[i];
            var next = coordPath[i + 1];

            // Get the direction we need to face to move to the next hex
            var targetFacing = current.GetDirectionToNeighbour(next);

            // Add turning positions if needed
            AddTurningPositions(current, currentFacing, targetFacing, result);

            // Add position in next hex with the target facing
            result.Add(new HexPosition(next, targetFacing));
            currentFacing = targetFacing;
        }

        // Finally, add any needed turning positions at the target
        AddTurningPositions(target.Coordinates, currentFacing, target.Facing, result);

        return result;

        // Local function to add turning positions
        void AddTurningPositions(HexCoordinates coords, HexDirection fromFacing, HexDirection toFacing, List<HexPosition> positions)
        {
            if (fromFacing == toFacing)
                return;

            var currentFacingInt = (int)fromFacing;
            var targetFacingInt = (int)toFacing;
            
            var clockwiseSteps = (targetFacingInt - currentFacingInt + 6) % 6;
            var counterClockwiseSteps = (currentFacingInt - targetFacingInt + 6) % 6;

            // Choose the shorter turning direction
            if (clockwiseSteps <= counterClockwiseSteps)
            {
                // Turn clockwise
                for (var step = 1; step <= clockwiseSteps; step++)
                {
                    var intermediateFacing = (HexDirection)((currentFacingInt + step) % 6);
                    positions.Add(new HexPosition(coords, intermediateFacing));
                }
            }
            else
            {
                // Turn counterclockwise
                for (var step = 1; step <= counterClockwiseSteps; step++)
                {
                    var intermediateFacing = (HexDirection)((currentFacingInt - step + 6) % 6);
                    positions.Add(new HexPosition(coords, intermediateFacing));
                }
            }
        }
    }

    /// <summary>
    /// Gets all valid hexes that can be reached with given movement points, considering facing
    /// </summary>
    public IEnumerable<(HexPosition position, int cost)> GetReachableHexes(
        HexPosition start,
        int maxMovementPoints)
    {
        var visited = new Dictionary<HexCoordinates, (HexPosition bestPos, int lowestCost)>();
        var toVisit = new Queue<HexPosition>();
        
        visited[start.Coordinates] = (start, 0);
        toVisit.Enqueue(start);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            var currentCost = visited[current.Coordinates].lowestCost;

            // For each adjacent hex
            foreach (var neighborCoord in current.Coordinates.GetAdjacentCoordinates())
            {
                // Skip if hex doesn't exist on map
                var neighborHex = GetHex(neighborCoord);
                if (neighborHex == null)
                    continue;

                // Get required facing to move to this hex
                var requiredFacing = current.Coordinates.GetDirectionToNeighbour(neighborCoord);
                
                // Calculate turning cost from current facing
                var turningCost = current.GetTurningCost(requiredFacing);
                
                // Calculate total cost including turning and movement
                var totalCost = currentCost + neighborHex.MovementCost + turningCost;
                
                if (totalCost > maxMovementPoints) // Exceeds movement points
                    continue;

                // If we haven't visited this hex or we found a cheaper path
                if (visited.ContainsKey(neighborCoord) && totalCost >= visited[neighborCoord].lowestCost) continue;
                var neighborPos = new HexPosition(neighborCoord, requiredFacing);
                visited[neighborCoord] = (neighborPos, totalCost);
                toVisit.Enqueue(neighborPos);
            }
        }

        return visited
            .Where(v => v.Key != start.Coordinates)
            .Select(v => (v.Value.bestPos, v.Value.lowestCost));
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
