namespace Sanet.MekForge.Core.Models.Game.Commands.Client;

public interface IClientCommand: IGameCommand
{
    Guid PlayerId { get; init; }
}