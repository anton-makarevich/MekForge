using System;
using System.Reactive.Subjects;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Protocol;

namespace Sanet.MekForge.Avalonia.Game.Protocol;

public class RxCommandPublisher : ICommandPublisher
{
    private readonly Subject<GameCommand> _commands = new();
    
    public void PublishCommand(GameCommand command)
    {
        _commands.OnNext(command);
    }
    
    public void Subscribe(Action<GameCommand> onCommandReceived)
    {
        _commands.Subscribe(onCommandReceived);
    }
}