using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Core.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
    IEnumerable<Hex> GetHexes();
}