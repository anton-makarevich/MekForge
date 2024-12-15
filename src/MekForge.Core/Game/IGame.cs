namespace Sanet.MekForge.Core.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
}