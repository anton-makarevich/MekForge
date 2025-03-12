using System;
using System.Reactive.Subjects;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Transport;

namespace Sanet.MekForge.Avalonia.Game.Transport;

public class RxCommandPublisher : ICommandPublisher
{
    private readonly Subject<IGameCommand> _commands = new();
    
    public void PublishCommand(IGameCommand command)
    {
        _commands.OnNext(command);
    }
    
    public void Subscribe(Action<IGameCommand> onCommandReceived)
    {
        _commands.Subscribe(onCommandReceived);
    }
}