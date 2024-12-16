using Sanet.MekForge.Core.Data;

namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record JoinGameCommand: ClientCommand
{
    public required string PlayerName { get; init; }
    public required List<UnitData> Units { get; init; }
}