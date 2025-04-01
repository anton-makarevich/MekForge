using Sanet.MakaMek.Core.Models.Map;
using Sanet.MakaMek.Core.Models.Map.Terrains;

namespace Sanet.MakaMek.Core.Utils;

public static class HexExtensions
{
    public static Hex WithTerrain(this Hex hex, Terrain terrain)
    {
        hex.AddTerrain(terrain);
        return hex;
    }
}
