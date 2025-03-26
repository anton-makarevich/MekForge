using NSubstitute;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Transport;
using Shouldly;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Game.Transport;

public class CommandPublisherTests
{
    private readonly CommandTransportAdapter _adapter;
    private readonly CommandPublisher _publisher;
    private Action<IGameCommand>? _adapterCallback;

    public CommandPublisherTests()
    {
        _adapter = Substitute.For<CommandTransportAdapter>(Substitute.For<Sanet.MekForge.Transport.ITransportPublisher>());
        
        // Capture the callback when Initialize is called
        _adapter.When(x => x.Initialize(Arg.Any<Action<IGameCommand>>()))
            .Do(x => _adapterCallback = x.Arg<Action<IGameCommand>>());
            
        _publisher = new CommandPublisher(_adapter);
    }

    [Fact]
    public void PublishCommand_DelegatesTo_Adapter()
    {
        // Arrange
        var command = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };

        // Act
        _publisher.PublishCommand(command);

        // Assert
        _adapter.Received(1).PublishCommand(command);
    }

    [Fact]
    public void Subscribe_RegistersCallback()
    {
        // Arrange
        var receivedCommand = false;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };

        // Act
        _publisher.Subscribe(cmd =>
        {
            cmd.ShouldBe(testCommand);
            receivedCommand = true;
        });

        // Simulate receiving a command through the adapter
        _adapterCallback.ShouldNotBeNull();
        _adapterCallback!(testCommand);

        // Assert
        receivedCommand.ShouldBeTrue();
    }

    [Fact]
    public void Subscribe_WithMultipleSubscribers_NotifiesAll()
    {
        // Arrange
        var subscriberCount = 3;
        var receivedCount = 0;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };

        // Act
        for (int i = 0; i < subscriberCount; i++)
        {
            _publisher.Subscribe(cmd =>
            {
                cmd.ShouldBe(testCommand);
                receivedCount++;
            });
        }

        // Simulate receiving a command through the adapter
        _adapterCallback.ShouldNotBeNull();
        _adapterCallback!(testCommand);

        // Assert
        receivedCount.ShouldBe(subscriberCount);
    }

    [Fact]
    public void Initialize_RegistersWithAdapter()
    {
        // Assert
        _adapter.Received(1).Initialize(Arg.Any<Action<IGameCommand>>());
    }
}
