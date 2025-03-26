using System;

namespace Sanet.MekForge.Transport;

/// <summary>
/// Represents a serializable transport message that can be sent between systems
/// without knowledge of the actual game command structure
/// </summary>
public class TransportMessage
{
    /// <summary>
    /// The type identifier of the command
    /// </summary>
    public string CommandType { get; set; } = string.Empty;
    
    /// <summary>
    /// The game origin identifier
    /// </summary>
    public Guid SourceId { get; set; }
    
    /// <summary>
    /// Serialized payload of the command data
    /// </summary>
    public string Payload { get; set; } = string.Empty;
    
    /// <summary>
    /// When the command was created
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Interface for transport publisher implementations
/// </summary>
public interface ITransportPublisher
{
    /// <summary>
    /// Publishes a transport message
    /// </summary>
    /// <param name="message">The message to publish</param>
    void PublishMessage(TransportMessage message);
    
    /// <summary>
    /// Subscribes to receive transport messages
    /// </summary>
    /// <param name="onMessageReceived">Action to call when a message is received</param>
    void Subscribe(Action<TransportMessage> onMessageReceived);
}
