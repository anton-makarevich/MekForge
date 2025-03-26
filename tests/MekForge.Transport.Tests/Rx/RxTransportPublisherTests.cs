using Sanet.MekForge.Transport;
using Sanet.MekForge.Transport.Rx;
using Shouldly;
using Xunit;

namespace Sanet.MekForge.Transport.Tests.Rx;

public class RxTransportPublisherTests
{
    [Fact]
    public void Subscribe_WhenMessagePublished_SubscriberReceivesMessage()
    {
        // Arrange
        var publisher = new RxTransportPublisher();
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
        
        // Assert
        receivedMessage.ShouldBeTrue();
    }

    [Fact]
    public void PublishMessage_WithMultipleSubscribers_AllSubscribersReceiveMessage()
    {
        // Arrange
        var publisher = new RxTransportPublisher();
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
        
        // Assert
        receivedCount.ShouldBe(subscriberCount);
    }
}
