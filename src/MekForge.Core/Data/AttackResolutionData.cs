using Sanet.MekForge.Core.Models.Game.Dice;

namespace Sanet.MekForge.Core.Data;

public record AttackResolutionData(
    int ToHitNumber,
    List<DiceResult> AttackRoll,
    bool IsHit,
    DiceResult? HitLocation = null);
