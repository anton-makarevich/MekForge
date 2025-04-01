using Sanet.MakaMek.Core.Models.Game.Dice;
using Sanet.MakaMek.Core.Models.Units;

namespace Sanet.MakaMek.Core.Data.Game;

/// <summary>
/// Represents a single hit location with its damage
/// </summary>
public record HitLocationData(
    PartLocation Location,
    int Damage,
    List<DiceResult> LocationRoll);