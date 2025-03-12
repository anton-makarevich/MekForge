using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Transport;

public interface ICommandPublisher
{
    void PublishCommand(IGameCommand command);
    void Subscribe(Action<IGameCommand> onCommandReceived);
}