using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public record JoinGameCommand: GameCommand
{
    public string PlayerName { get; init; }
    public List<UnitData> Units { get; init; }
}