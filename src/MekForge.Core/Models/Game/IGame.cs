using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using System.Reactive;

namespace Sanet.MekForge.Core.Models.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
    IEnumerable<Hex> GetHexes();
    int Turn { get; }
    PhaseNames TurnPhase { get; }
    IPlayer? ActivePlayer { get; }
    int UnitsToPlayCurrentStep { get; }

    IObservable<int> TurnChanges { get; }
    IObservable<PhaseNames> PhaseChanges { get; }
    IObservable<IPlayer?> ActivePlayerChanges { get; }
    IObservable<int> UnitsToPlayChanges { get; }
}