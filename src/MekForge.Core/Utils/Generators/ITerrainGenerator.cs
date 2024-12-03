using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Core.Utils.Generators;

/// <summary>
/// Interface for terrain generation strategies
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>
    /// Generates terrain for a hex at the given coordinates
    /// </summary>
    Hex Generate(HexCoordinates coordinates);
}
