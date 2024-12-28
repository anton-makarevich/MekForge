using FluentAssertions;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.ViewModels.Wrappers;

namespace Sanet.MekForge.Core.Tests.ViewModels.Wrappers;

public class PlayerViewModelTests
{
    [Fact]
    public void AddUnit_ShouldAddUnitToPlayer()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"));
        var unit = MechFactoryTests.CreateDummyMechData(); // Create a new unit
    
        // Act
        playerViewModel.AddUnitCommand.Execute(unit);
    
        // Assert
        playerViewModel.Units.Should().Contain(unit);
    }
}