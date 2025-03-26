using System;
using System.Reactive.Subjects;
using Sanet.MekForge.Transport;

namespace Sanet.MekForge.Transport.Rx;

/// <summary>
/// Implementation of ITransportPublisher using Reactive Extensions
/// </summary>
public class RxTransportPublisher : ITransportPublisher
{
    private readonly Subject<TransportMessage> _messages = new();
    
    /// <summary>
    /// Publishes a transport message to all subscribers
    /// </summary>
    public void PublishMessage(TransportMessage message)
    {
        _messages.OnNext(message);
    }
    
    /// <summary>
    /// Subscribes to receive transport messages
    /// </summary>
    public void Subscribe(Action<TransportMessage> onMessageReceived)
    {
        _messages.Subscribe(onMessageReceived);
    }
}
