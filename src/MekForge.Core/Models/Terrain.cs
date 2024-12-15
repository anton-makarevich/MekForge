using Sanet.MekForge.Core.Models.Terrains;

namespace Sanet.MekForge.Core.Models;

/// <summary>
/// Base class for all terrain types
/// </summary>
public abstract class Terrain
{
    /// <summary>
    /// Unique identifier for this terrain type
    /// </summary>
    public abstract string Id { get; }

        /// <summary>
    /// Fixed height of this terrain type
    /// </summary>
    public abstract int Height { get; }

    /// <summary>
    /// Movement cost modifier for this terrain
    /// </summary>
    public abstract int TerrainFactor { get; }

    public static Terrain GetTerrainType(string terrainType)
    {
        return terrainType switch
        {
            "Clear" => new ClearTerrain(),
            "LightWoods" => new LightWoodsTerrain(),
            "HeavyWoods" => new HeavyWoodsTerrain(),
            _ => throw new ArgumentException($"Unknown terrain type: {terrainType}")
        };
    }
}