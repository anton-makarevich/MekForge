using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Map.Terrains;

namespace Sanet.MekForge.Core.Utils;

public static class HexExtensions
{
    public static Hex WithTerrain(this Hex hex, Terrain terrain)
    {
        hex.AddTerrain(terrain);
        return hex;
    }
}
