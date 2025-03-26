using System.Threading.Channels;
using Sanet.MekForge.Transport;

namespace Sanet.MekForge.Transport.Channel;

/// <summary>
/// Implementation of ITransportPublisher using System.Threading.Channels
/// </summary>
public class ChannelTransportPublisher : ITransportPublisher, IDisposable
{
    private readonly Channel<TransportMessage> _channel;
    private readonly List<Action<TransportMessage>> _subscribers;
    private readonly CancellationTokenSource _cts;
    private readonly Task _processTask;

    /// <summary>
    /// Creates a new instance of the ChannelTransportPublisher
    /// </summary>
    /// <param name="capacity">The maximum number of messages that can be buffered</param>
    public ChannelTransportPublisher(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        
        _channel = System.Threading.Channels.Channel.CreateBounded<TransportMessage>(options);
        _subscribers = new List<Action<TransportMessage>>();
        _cts = new CancellationTokenSource();
        
        // Start processing messages in background
        _processTask = ProcessMessagesAsync(_cts.Token);
    }

    /// <summary>
    /// Publishes a transport message to all subscribers
    /// </summary>
    public async void PublishMessage(TransportMessage message)
    {
        try
        {
            await _channel.Writer.WriteAsync(message, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Channel has been closed
        }
    }

    /// <summary>
    /// Subscribes to receive transport messages
    /// </summary>
    public void Subscribe(Action<TransportMessage> onMessageReceived)
    {
        lock (_subscribers)
        {
            _subscribers.Add(onMessageReceived);
        }
    }
    
    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var message))
                {
                    NotifySubscribers(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error processing messages: {ex.Message}");
        }
    }
    
    private void NotifySubscribers(TransportMessage message)
    {
        Action<TransportMessage>[] subscribersSnapshot;
        
        lock (_subscribers)
        {
            subscribersSnapshot = _subscribers.ToArray();
        }
        
        foreach (var subscriber in subscribersSnapshot)
        {
            try
            {
                subscriber(message);
            }
            catch (Exception ex)
            {
                // Log subscriber error
                Console.WriteLine($"Error in subscriber: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        // Cancel and wait for processing to complete
        _cts.Cancel();
        try
        {
            _processTask.Wait(1000); // Wait up to 1 second
        }
        catch
        {
            // Ignore exceptions during cleanup
        }
        
        _cts.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
