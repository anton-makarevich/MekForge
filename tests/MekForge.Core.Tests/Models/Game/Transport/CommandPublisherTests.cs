using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.Transport;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Game.Transport;

public class CommandPublisherTests
{
    private readonly CommandPublisher _publisher;
    private readonly ITransportPublisher _transportPublisher = Substitute.For<ITransportPublisher>();
    private Action<TransportMessage>? _transportCallback;

    public CommandPublisherTests()
    {
        // Capture the Subscribe callback
        _transportPublisher.When(x => x.Subscribe(Arg.Any<Action<TransportMessage>>()))
            .Do(x => _transportCallback = x.Arg<Action<TransportMessage>>());
        
        var adapter = new CommandTransportAdapter(_transportPublisher);
        _publisher = new CommandPublisher(adapter);
    }

    [Fact]
    public void PublishCommand_DelegatesTo_Adapter()
    {
        // Arrange
        var command = new TurnIncrementedCommand
        {
            GameOriginId = Guid.NewGuid()
        };

        // Act
        _publisher.PublishCommand(command);

        // Assert
        _transportPublisher.Received().PublishMessage(Arg.Any<TransportMessage>());
    }

    [Fact]
    public void Subscribe_ReceivesCommands_WhenCommandReceived()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var receivedCommand = null as IGameCommand;
        
        _publisher.Subscribe(cmd => receivedCommand = cmd);
        
        var message = new TransportMessage
        {
            MessageType = "TurnIncrementedCommand",
            SourceId = sourceId,
            Timestamp = timestamp,
            Payload = $"{{\"GameOriginId\":\"{sourceId}\",\"Timestamp\":\"{timestamp:o}\"}}"
        };

        // Act - simulate receiving a message from transport
        _transportCallback.ShouldNotBeNull();
        _transportCallback!(message);

        // Assert
        receivedCommand.ShouldNotBeNull();
        receivedCommand.ShouldBeOfType<TurnIncrementedCommand>();
        receivedCommand.GameOriginId.ShouldBe(sourceId);
        receivedCommand.Timestamp.ShouldBe(timestamp);
    }

    [Fact]
    public void Subscribe_MultipleSubscribers_AllReceiveCommands()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var receivedBySubscriber1 = false;
        var receivedBySubscriber2 = false;
        
        _publisher.Subscribe(_ => receivedBySubscriber1 = true);
        _publisher.Subscribe(_ => receivedBySubscriber2 = true);
        
        var message = new TransportMessage
        {
            MessageType = "TurnIncrementedCommand",
            SourceId = sourceId,
            Timestamp = timestamp,
            Payload = $"{{\"GameOriginId\":\"{sourceId}\",\"Timestamp\":\"{timestamp:o}\"}}"
        };

        // Act
        _transportCallback.ShouldNotBeNull();
        _transportCallback!(message);

        // Assert
        receivedBySubscriber1.ShouldBeTrue();
        receivedBySubscriber2.ShouldBeTrue();
    }

    [Fact]
    public void Subscribe_ErrorInOneSubscriber_DoesNotAffectOthers()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var receivedBySubscriber2 = false;
        
        // First subscriber throws an exception
        _publisher.Subscribe(_ => throw new Exception("Test exception"));
        
        // Second subscriber should still be called
        _publisher.Subscribe(_ => receivedBySubscriber2 = true);
        
        var message = new TransportMessage
        {
            MessageType = "TurnIncrementedCommand",
            SourceId = sourceId,
            Timestamp = timestamp,
            Payload = $"{{\"GameOriginId\":\"{sourceId}\",\"Timestamp\":\"{timestamp:o}\"}}"
        };

        // Act - this should not throw despite the first subscriber throwing
        _transportCallback.ShouldNotBeNull();
        _transportCallback!(message);

        // Assert
        receivedBySubscriber2.ShouldBeTrue();
    }

    [Fact]
    public void Subscribe_UnknownCommandType_DoesNotCallSubscribers()
    {
        // Arrange
        var subscriberCalled = false;
        _publisher.Subscribe(_ => subscriberCalled = true);
        
        var message = new TransportMessage
        {
            MessageType = "UnknownCommand",
            SourceId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Payload = "{}"
        };

        // Act
        _transportCallback.ShouldNotBeNull();
        _transportCallback!(message);

        // Assert
        subscriberCalled.ShouldBeFalse();
    }
}
