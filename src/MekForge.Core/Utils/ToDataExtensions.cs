using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Core.Utils;

public static class ToDataExtensions
{
    public static HexCoordinateData ToData(this HexCoordinates hex) => new(hex.Q, hex.R);
}