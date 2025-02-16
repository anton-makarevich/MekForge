using Sanet.MekForge.Avalonia.Game.Transport;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Services.Localization;
using Shouldly;

namespace MekForge.Avalonia.Tests.Game.Transport;

public class ChannelCommandPublisherTests
{
    [Fact]
    public async Task Subscribe_WhenCommandPublished_SubscriberReceivesCommand()
    {
        // Arrange
        using var publisher = new ChannelCommandPublisher();
        var receivedCommand = false;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        publisher.Subscribe(cmd =>
        {
            cmd.ShouldBe(testCommand);
            receivedCommand = true;
        });
        
        publisher.PublishCommand(testCommand);
        
        // Assert - wait a bit for async processing
        await Task.Delay(100);
        receivedCommand.ShouldBeTrue();
    }
    
    [Fact]
    public async Task Subscribe_MultipleSubscribers_AllReceiveCommand()
    {
        // Arrange
        using var publisher = new ChannelCommandPublisher();
        var subscriber1Received = false;
        var subscriber2Received = false;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        publisher.Subscribe(_ => subscriber1Received = true);
        publisher.Subscribe(_ => subscriber2Received = true);
        
        publisher.PublishCommand(testCommand);
        
        // Assert
        await Task.Delay(100);
        subscriber1Received.ShouldBeTrue();
        subscriber2Received.ShouldBeTrue();
    }
    
    [Fact]
    public async Task Subscribe_SubscriberThrowsException_OtherSubscriberStillReceivesCommand()
    {
        // Arrange
        using var publisher = new ChannelCommandPublisher();
        var goodSubscriberReceived = false;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };
        
        // Act
        publisher.Subscribe(_ => throw new Exception("Test exception"));
        publisher.Subscribe(_ => goodSubscriberReceived = true);
        
        publisher.PublishCommand(testCommand);
        
        // Assert
        await Task.Delay(100);
        goodSubscriberReceived.ShouldBeTrue();
    }
    
    [Fact]
    public async Task Dispose_WhenCalled_StopsProcessingNewCommands()
    {
        // Arrange
        var publisher = new ChannelCommandPublisher();
        var receivedAfterDispose = false;
        var testCommand = new TestCommand
        {
            GameOriginId = Guid.NewGuid()
        };
        
        publisher.Subscribe(_ => receivedAfterDispose = true);
        
        // Act
        publisher.Dispose();
        publisher.PublishCommand(testCommand);
        
        // Assert
        await Task.Delay(100);
        receivedAfterDispose.ShouldBeFalse();
    }
    
    [Fact]
    public async Task PublishCommand_WhenChannelFull_EventuallyProcessesAllCommands()
    {
        // Arrange
        using var publisher = new ChannelCommandPublisher(2); // Small buffer
        var processedCount = 0;
        var totalCommands = 10;
        
        publisher.Subscribe(_ => processedCount++);
        
        // Act
        for (var i = 0; i < totalCommands; i++)
        {
            publisher.PublishCommand(new TestCommand
            {
                GameOriginId = Guid.NewGuid()
            });
        }
        
        // Assert - give enough time for all commands to be processed
        await Task.Delay(500);
        processedCount.ShouldBe(totalCommands);
    }
}

// Test command class for testing
public record TestCommand : GameCommand
{
    public override string Format(ILocalizationService localizationService, IGame game) => "Test";
}
