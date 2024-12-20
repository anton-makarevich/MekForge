using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Models.Game.Transport;

public interface ICommandPublisher
{
    void PublishCommand(GameCommand command);
    void Subscribe(Action<GameCommand> onCommandReceived);
}