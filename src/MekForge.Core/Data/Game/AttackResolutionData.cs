using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Core.Data.Game;

public record AttackResolutionData(
    int ToHitNumber,
    List<DiceResult> AttackRoll,
    bool IsHit,
    FiringArc? AttackDirection = null,
    AttackHitLocationsData? HitLocationsData = null);
