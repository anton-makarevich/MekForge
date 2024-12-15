using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public record JoinGameCommand(string PlayerName, List<UnitData> Units) : GameCommand;