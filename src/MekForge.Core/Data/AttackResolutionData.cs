using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Data;

public record AttackResolutionData(
    int ToHitNumber,
    List<DiceResult> AttackRoll,
    bool IsHit,
    PartLocation? HitLocation = null);
