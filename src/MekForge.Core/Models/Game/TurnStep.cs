using Sanet.MekForge.Core.Models.Game.Players;

namespace Sanet.MekForge.Core.Models.Game;

public record TurnStep(IPlayer Player, int UnitsToMove);