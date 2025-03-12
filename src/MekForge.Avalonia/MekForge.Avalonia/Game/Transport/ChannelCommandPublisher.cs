using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Transport;

namespace Sanet.MekForge.Avalonia.Game.Transport;

public class ChannelCommandPublisher : ICommandPublisher, IDisposable
{
    private readonly Channel<IGameCommand> _channel;
    private readonly List<Action<IGameCommand>> _subscribers;
    private readonly CancellationTokenSource _cts;
    private readonly Task _processTask;

    public ChannelCommandPublisher(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        
        _channel = Channel.CreateBounded<IGameCommand>(options);
        _subscribers = new List<Action<IGameCommand>>();
        _cts = new CancellationTokenSource();
        
        // Start processing messages in background
        _processTask = ProcessMessagesAsync(_cts.Token);
    }

    public async void PublishCommand(IGameCommand command)
    {
        try
        {
            await _channel.Writer.WriteAsync(command, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Channel has been closed
        }
    }

    public void Subscribe(Action<IGameCommand> onCommandReceived)
    {
        lock (_subscribers)
        {
            _subscribers.Add(onCommandReceived);
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var command in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                Action<IGameCommand>[] currentSubscribers;
                lock (_subscribers)
                {
                    currentSubscribers = _subscribers.ToArray();
                }

                foreach (var subscriber in currentSubscribers)
                {
                    try
                    {
                        subscriber(command);
                    }
                    catch (Exception)
                    {
                        // Log error but continue processing for other subscribers
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Channel has been closed
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _channel.Writer.Complete();
        _processTask.Wait(TimeSpan.FromSeconds(1));
        _cts.Dispose();
    }
}
