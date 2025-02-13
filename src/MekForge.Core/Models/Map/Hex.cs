using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents a single hex on the game map
/// </summary>
public class Hex
{
    public HexCoordinates Coordinates { get; }
    public int Level { get; private set; }
    private readonly Dictionary<string, Terrain> _terrains = new();
    public string? Theme { get; set; }

    public Hex(HexCoordinates coordinates, int level = 0)
    {
        Coordinates = coordinates;
        Level = level;
    }

    public void AddTerrain(Terrain terrain)
    {
        _terrains[terrain.Id] = terrain;
    }

    public void RemoveTerrain(string terrainId)
    {
        _terrains.Remove(terrainId);
    }

    public bool HasTerrain(string terrainId) => _terrains.ContainsKey(terrainId);

    public Terrain? GetTerrain(string terrainId) =>
        _terrains.GetValueOrDefault(terrainId);

    public IEnumerable<Terrain> GetTerrains() => _terrains.Values;

    public int GetCeiling()
    {
        var maxTerrainHeight = _terrains.Count != 0
            ? _terrains.Values.Max(t => t.Height) 
            : 0;
        return Level + maxTerrainHeight;
    }

    /// <summary>
    /// Gets the movement cost for entering this hex (highest terrain factor)
    /// </summary>
    public int MovementCost => _terrains.Count != 0 
        ? _terrains.Values.Max(t => t.MovementCost)
        : 1; // Default cost for empty hex

    public bool IsHighlighted { get; set; }

    public string[] GetTerrainTypes()
    {
        return _terrains.Values.Select(t => t.Id).ToArray(); 
    }
    
    public HexData ToData()
    {
        return new HexData
        {
            Coordinates = Coordinates.ToData(),
            TerrainTypes = GetTerrainTypes(),
            Level = Level
        };
    }
}