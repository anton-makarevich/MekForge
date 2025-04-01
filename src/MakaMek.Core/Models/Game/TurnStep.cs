using Sanet.MakaMek.Core.Models.Game.Players;

namespace Sanet.MakaMek.Core.Models.Game;

public record TurnStep(IPlayer Player, int UnitsToMove);