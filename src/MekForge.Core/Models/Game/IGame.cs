namespace Sanet.MekForge.Core.Models.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
    IEnumerable<Hex> GetHexes();
    int Turn { get; }
    Phase TurnPhase { get; }

    IPlayer? ActivePlayer { get; }
}