using Sanet.MekForge.Avalonia.Game.Transport;
using Sanet.MekForge.Core.Models.Game.Commands;
using Shouldly;

namespace MekForge.Avalonia.Tests.Game.Transport;

public class RxCommandPublisherTests
{
    [Fact]
    public void Subscribe_WhenCommandPublished_SubscriberReceivesCommand()
    {
        // Arrange
        var publisher = new RxCommandPublisher();
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
        
        // Assert
        receivedCommand.ShouldBeTrue();
    }
    
    [Fact]
    public void Subscribe_MultipleSubscribers_AllReceiveCommand()
    {
        // Arrange
        var publisher = new RxCommandPublisher();
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
        subscriber1Received.ShouldBeTrue();
        subscriber2Received.ShouldBeTrue();
    }
    
    [Fact]
    public void Subscribe_MultipleCommands_AllCommandsDeliveredInOrder()
    {
        // Arrange
        var publisher = new RxCommandPublisher();
        var receivedCommands = new List<IGameCommand>();
        var command1 = new TestCommand { GameOriginId = Guid.NewGuid() };
        var command2 = new TestCommand { GameOriginId = Guid.NewGuid() };
        var command3 = new TestCommand { GameOriginId = Guid.NewGuid() };
        
        // Act
        publisher.Subscribe(cmd => receivedCommands.Add(cmd));
        
        publisher.PublishCommand(command1);
        publisher.PublishCommand(command2);
        publisher.PublishCommand(command3);
        
        // Assert
        receivedCommands.Count.ShouldBe(3);
        receivedCommands[0].ShouldBe(command1);
        receivedCommands[1].ShouldBe(command2);
        receivedCommands[2].ShouldBe(command3);
    }
    
    [Fact]
    public void Subscribe_LateSubscriber_OnlyReceivesNewCommands()
    {
        // Arrange
        var publisher = new RxCommandPublisher();
        var receivedCommands = new List<IGameCommand>();
        var command1 = new TestCommand { GameOriginId = Guid.NewGuid() };
        var command2 = new TestCommand { GameOriginId = Guid.NewGuid() };
        
        // Act - publish first command before subscribing
        publisher.PublishCommand(command1);
        
        // Subscribe after first command
        publisher.Subscribe(cmd => receivedCommands.Add(cmd));
        
        // Publish second command
        publisher.PublishCommand(command2);
        
        // Assert
        receivedCommands.Count.ShouldBe(1);
        receivedCommands[0].ShouldBe(command2);
    }
}
