using FluentAssertions;
using Sanet.MekForge.Core.Models.Game;
using Sanet.MekForge.Core.Tests.Data;
using Sanet.MekForge.Core.ViewModels.Wrappers;

namespace Sanet.MekForge.Core.Tests.ViewModels.Wrappers;

public class PlayerViewModelTests
{
    [Fact]
    public void AddUnit_ShouldAddUnitToPlayer_IfSelectedUnitIsNotNull()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[]);
        var unit = MechFactoryTests.CreateDummyMechData(); // Create a new unit
        playerViewModel.SelectedUnit = unit;
    
        // Act
        playerViewModel.AddUnitCommand.Execute(null);
    
        // Assert
        playerViewModel.Units.First().Chassis.Should().Be(unit.Chassis);
    }

    [Fact]
    public void AddUnit_ShouldNotAddUnitToPlayer_IfSelectedUnitIsNull()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[]);
    
        // Act
        playerViewModel.AddUnitCommand.Execute(null);
    
        // Assert
        playerViewModel.Units.Count.Should().Be(0);
    }
    
    [Fact]
    public void Name_ShouldReturnPlayerName()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[]);
    
        // Act
        var name = playerViewModel.Name;
    
        // Assert
        name.Should().Be("Player1");
    }
    
    [Fact]
    public void CanAddUnit_ShouldReturnTrue_IfSelectedUnitIsNotNull()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[]);
        var unit = MechFactoryTests.CreateDummyMechData(); // Create a new unit
        playerViewModel.SelectedUnit = unit;
    
        // Act
        var canAddUnit = playerViewModel.CanAddUnit;
    
        // Assert
        canAddUnit.Should().BeTrue();
    }
    
    [Fact]
    public void CanAddUnit_ShouldReturnFalse_IfSelectedUnitIsNull()
    {
        // Arrange
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[]);
    
        // Act
        var canAddUnit = playerViewModel.CanAddUnit;
    
        // Assert
        canAddUnit.Should().BeFalse();
    } 
    
    [Fact]
    public void AvailableUnits_ShouldHaveCorrectValue()
    {
        // Arrange
        var unit = MechFactoryTests.CreateDummyMechData(); // Create a new unit
        var playerViewModel = new PlayerViewModel(new Player(Guid.NewGuid(), "Player1"),[unit]);
    
        // Act
        var availableUnits = playerViewModel.AvailableUnits.ToList();
    
        // Assert
        availableUnits.Contains(unit).Should().BeTrue();
    }
}