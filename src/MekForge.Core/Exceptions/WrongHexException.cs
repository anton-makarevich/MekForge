using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Exceptions;

public class WrongHexException : Exception
{
    public HexCoordinates Coordinates { get; }

    public WrongHexException(HexCoordinates coordinates, string message) : base(message)
    {
        Coordinates = coordinates;
    }
}
