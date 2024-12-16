namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public record UpdatePlayerStatusCommand: ClientCommand
{
    public required PlayerStatus PlayerStatus { get; init; }
}