using Sanet.MekForge.Core.Models.Game.Dice;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Data.Game;

/// <summary>
/// Represents a single hit location with its damage
/// </summary>
public record HitLocationData(
    PartLocation Location,
    int Damage,
    List<DiceResult> LocationRoll);