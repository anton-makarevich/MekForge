using NSubstitute;
using Sanet.MakaMek.Core.Exceptions;
using Sanet.MakaMek.Core.Models.Game.Commands;
using Sanet.MakaMek.Core.Models.Game.Commands.Server;
using Sanet.MakaMek.Core.Models.Game.Transport;
using Sanet.Transport;
using Shouldly;

namespace Sanet.MakaMek.Core.Tests.Models.Game.Transport;

public class CommandTransportAdapterTests
{
    private readonly ITransportPublisher _transportPublisher;
    private readonly CommandTransportAdapter _adapter;

    public CommandTransportAdapterTests()
    {
        _transportPublisher = Substitute.For<ITransportPublisher>();
        _adapter = new CommandTransportAdapter(_transportPublisher);
    }

    [Fact]
    public void PublishCommand_ConvertsToTransportMessage()
    {
        // Arrange
        var command = new TurnIncrementedCommand
        {
            GameOriginId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };
        
        TransportMessage? capturedMessage = null;
        _transportPublisher.When(x => x.PublishMessage(Arg.Any<TransportMessage>()))
            .Do(x => capturedMessage = x.Arg<TransportMessage>());

        // Act
        _adapter.PublishCommand(command);

        // Assert
        _transportPublisher.Received(1).PublishMessage(Arg.Any<TransportMessage>());
        capturedMessage.ShouldNotBeNull();
        capturedMessage!.MessageType.ShouldBe("TurnIncrementedCommand");
        capturedMessage.SourceId.ShouldBe(command.GameOriginId);
        capturedMessage.Timestamp.ShouldBe(command.Timestamp);
        capturedMessage.Payload.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Initialize_DeserializesReceivedCommands()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var message = new TransportMessage
        {
            MessageType = "TurnIncrementedCommand",
            SourceId = sourceId,
            Timestamp = timestamp,
            Payload = $"{{\"GameOriginId\":\"{sourceId}\",\"Timestamp\":\"{timestamp:o}\"}}"
        };

        Action<TransportMessage>? subscribedCallback = null;
        _transportPublisher.When(x => x.Subscribe(Arg.Any<Action<TransportMessage>>()))
            .Do(x => {
                subscribedCallback = x.Arg<Action<TransportMessage>>();
            });

        IGameCommand? receivedCommand = null;
        
        // Act
        _adapter.Initialize(cmd => receivedCommand = cmd);
        
        // Now trigger the callback manually
        subscribedCallback!.Invoke(message);

        // Assert
        receivedCommand.ShouldNotBeNull();
        receivedCommand.ShouldBeOfType<TurnIncrementedCommand>();
        receivedCommand.GameOriginId.ShouldBe(sourceId);
        receivedCommand.Timestamp.ShouldBe(timestamp);
    }

    [Fact]
    public void Initialize_WithUnknownCommandType_ThrowsException()
    {
        // Arrange
        var message = new TransportMessage
        {
            MessageType = "UnknownCommand",
            SourceId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Payload = "{}"
        };

        Action<TransportMessage>? subscribedCallback = null;
        _transportPublisher.When(x => x.Subscribe(Arg.Any<Action<TransportMessage>>()))
            .Do(x => {
                subscribedCallback = x.Arg<Action<TransportMessage>>();
            });
        
        // Act & Assert
        _adapter.Initialize(_ => { });
        
        // Now trigger the callback manually
        Should.Throw<UnknownCommandTypeException>(() => subscribedCallback!.Invoke(message))
            .CommandType.ShouldBe("UnknownCommand");
    }

    [Fact]
    public void DeserializeCommand_WithInvalidJson_ThrowsException()
    {
        // Arrange
        var message = new TransportMessage
        {
            MessageType = "TurnIncrementedCommand",
            SourceId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Payload = "{ invalid json }"
        };

        Action<TransportMessage>? subscribedCallback = null;
        _transportPublisher.When(x => x.Subscribe(Arg.Any<Action<TransportMessage>>()))
            .Do(x => {
                subscribedCallback = x.Arg<Action<TransportMessage>>();
            });
        
        // Act & Assert
        _adapter.Initialize(_ => { });
        
        // Now trigger the callback manually
        Should.Throw<Exception>(() => subscribedCallback!.Invoke(message));
    }
}
