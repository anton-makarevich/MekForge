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
}