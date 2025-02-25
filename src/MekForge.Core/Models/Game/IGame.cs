using Sanet.MekForge.Core.Models.Game.Phases;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Game.Combat;

namespace Sanet.MekForge.Core.Models.Game;

public interface IGame
{
    IReadOnlyList<IPlayer> Players { get; }
    int Turn { get; }
    PhaseNames TurnPhase { get; }
    IPlayer? ActivePlayer { get; }
    int UnitsToPlayCurrentStep { get; }

    IObservable<int> TurnChanges { get; }
    IObservable<PhaseNames> PhaseChanges { get; }
    IObservable<IPlayer?> ActivePlayerChanges { get; }
    IObservable<int> UnitsToPlayChanges { get; }
    
    BattleMap BattleMap { get; }
    
    Guid Id { get; }
    
    IToHitCalculator ToHitCalculator { get; }
}