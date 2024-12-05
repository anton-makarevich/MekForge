using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class ComponentTests
{
    private class TestComponent : Component
    {
        public TestComponent(string name, int slots) : base(name, slots)
        {
        }
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var component = new TestComponent("Test Component", 2);

        // Assert
        component.Name.Should().Be("Test Component");
        component.Slots.Should().Be(2);
        component.IsDestroyed.Should().BeFalse();
        component.IsActive.Should().BeTrue();
        component.FirstOccupiedSlot.Should().Be(-1);
        component.LastOccupiedSlot.Should().Be(-1);
        component.IsMounted.Should().BeFalse();
    }

    [Fact]
    public void Mount_SetsCorrectSlotPositions()
    {
        // Arrange
        var component = new TestComponent("Test Component", 3);

        // Act
        component.Mount(2);

        // Assert
        component.FirstOccupiedSlot.Should().Be(2);
        component.LastOccupiedSlot.Should().Be(4); // 2 + 3 - 1
        component.IsMounted.Should().BeTrue();
    }

    [Fact]
    public void ApplyDamage_SetsIsDestroyedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component", 2);

        // Act
        component.ApplyDamage();

        // Assert
        component.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Activate_DeactivateTogglesIsActive()
    {
        // Arrange
        var component = new TestComponent("Test Component", 2);
        
        // Act & Assert
        component.IsActive.Should().BeTrue(); // Default state
        
        component.Deactivate();
        component.IsActive.Should().BeFalse();
        
        component.Activate();
        component.IsActive.Should().BeTrue();
    }
}
