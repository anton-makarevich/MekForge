using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Transport;
using Sanet.MekForge.Core.Services.Localization;
using Sanet.MekForge.Transport;
using Shouldly;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Game.Transport;

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
        var command = new TestCommand
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
        capturedMessage!.CommandType.ShouldBe("TestCommand");
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
            CommandType = "TestCommand",
            SourceId = sourceId,
            Timestamp = timestamp,
            Payload = $"{{\"GameOriginId\":\"{sourceId}\",\"Timestamp\":\"{timestamp:o}\"}}"
        };

        IGameCommand? receivedCommand = null;
        _adapter.Initialize(cmd => receivedCommand = cmd);

        // Act - simulate receiving a message from the transport
        _transportPublisher.Subscribe(Arg.Any<Action<TransportMessage>>())
            .Returns(x =>
            {
                var callback = x.Arg<Action<TransportMessage>>();
                callback(message);
                return null;
            });

        // Assert
        receivedCommand.ShouldNotBeNull();
        receivedCommand.ShouldBeOfType<TestCommand>();
        receivedCommand.GameOriginId.ShouldBe(sourceId);
        receivedCommand.Timestamp.ShouldBe(timestamp);
    }

    [Fact]
    public void Initialize_WithUnknownCommandType_ShouldNotCallCallback()
    {
        // Arrange
        var message = new TransportMessage
        {
            CommandType = "UnknownCommand",
            SourceId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Payload = "{}"
        };

        bool callbackInvoked = false;
        _adapter.Initialize(_ => callbackInvoked = true);

        // Act - simulate receiving a message from the transport
        _transportPublisher.Subscribe(Arg.Any<Action<TransportMessage>>())
            .Returns(x =>
            {
                var callback = x.Arg<Action<TransportMessage>>();
                callback(message);
                return null;
            });

        // Assert
        callbackInvoked.ShouldBeFalse();
    }
}
