using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands;

public record JoinGameCommand: GameCommand
{
    public required string PlayerName { get; init; }
    public required List<UnitData> Units { get; init; }
}