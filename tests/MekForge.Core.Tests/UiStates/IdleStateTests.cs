using FluentAssertions;
using Sanet.MekForge.Core.UiStates;

namespace Sanet.MekForge.Core.Tests.UiStates;

public class IdleStateTests
{
    [Fact]
    public void ActionLabel_ShouldBeWait()
    {
        // Arrange
        var sut = new IdleState();
        
        // Assert
        sut.ActionLabel.Should().Be("Wait");
    }
    
    [Fact]
    public void IsActionRequired_ShouldBeFalse()
    {
        // Arrange
        var sut = new IdleState();
        
        // Assert
        sut.IsActionRequired.Should().BeFalse();
    }
}