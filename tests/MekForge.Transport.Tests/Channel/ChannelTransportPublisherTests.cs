using Sanet.MekForge.Transport;
using Sanet.MekForge.Transport.Channel;
using Shouldly;
using Xunit;

namespace Sanet.MekForge.Transport.Tests.Channel;

public class ChannelTransportPublisherTests
{
    [Fact]
    public async Task Subscribe_WhenMessagePublished_SubscriberReceivesMessage()
    {
        // Arrange
        using var publisher = new ChannelTransportPublisher();
        var receivedMessage = false;
        var testMessage = new TransportMessage
        {
            CommandType = "TestCommand",
            SourceId = Guid.NewGuid(),
            Payload = "{}",
            Timestamp = DateTime.UtcNow
        };
        
        // Act
        publisher.Subscribe(msg =>
        {
            msg.ShouldBe(testMessage);
            receivedMessage = true;
        });
        
        publisher.PublishMessage(testMessage);
        
        // Assert - wait a bit for async processing
        await Task.Delay(100);
        receivedMessage.ShouldBeTrue();
    }

    [Fact]
    public async Task PublishMessage_WithMultipleSubscribers_AllSubscribersReceiveMessage()
    {
        // Arrange
        using var publisher = new ChannelTransportPublisher();
        var subscriberCount = 3;
        var receivedCount = 0;
        var testMessage = new TransportMessage
        {
            CommandType = "TestCommand",
            SourceId = Guid.NewGuid(),
            Payload = "{}",
            Timestamp = DateTime.UtcNow
        };
        
        // Act
        for (int i = 0; i < subscriberCount; i++)
        {
            publisher.Subscribe(msg =>
            {
                msg.ShouldBe(testMessage);
                receivedCount++;
            });
        }
        
        publisher.PublishMessage(testMessage);
        
        // Assert - wait a bit for async processing
        await Task.Delay(100);
        receivedCount.ShouldBe(subscriberCount);
    }
    
    [Fact]
    public async Task Dispose_StopsProcessingMessages()
    {
        // Arrange
        var publisher = new ChannelTransportPublisher();
        var receivedCount = 0;
        publisher.Subscribe(_ => receivedCount++);
        
        // Act
        for (int i = 0; i < 5; i++)
        {
            publisher.PublishMessage(new TransportMessage
            {
                CommandType = "TestCommand",
                SourceId = Guid.NewGuid(),
                Payload = "{}",
                Timestamp = DateTime.UtcNow
            });
        }
        
        // Wait for messages to be processed
        await Task.Delay(100);
        var initialCount = receivedCount;
        
        // Dispose the publisher
        publisher.Dispose();
        
        // Try to publish more messages after dispose
        for (int i = 0; i < 5; i++)
        {
            publisher.PublishMessage(new TransportMessage
            {
                CommandType = "TestCommand",
                SourceId = Guid.NewGuid(),
                Payload = "{}",
                Timestamp = DateTime.UtcNow
            });
        }
        
        // Wait to see if any more messages are processed
        await Task.Delay(100);
        
        // Assert
        receivedCount.ShouldBe(initialCount);
    }
}
