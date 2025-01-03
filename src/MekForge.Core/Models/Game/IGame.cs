using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Models.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
    IEnumerable<Hex> GetHexes();
    int Turn { get; }
    PhaseNames TurnPhase { get; }

    IPlayer? ActivePlayer { get; }
    int UnitsToMoveCurrentStep { get; }
}