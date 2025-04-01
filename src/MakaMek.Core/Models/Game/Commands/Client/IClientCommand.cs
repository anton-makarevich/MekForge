namespace Sanet.MakaMek.Core.Models.Game.Commands.Client;

public interface IClientCommand: IGameCommand
{
    Guid PlayerId { get; init; }
}