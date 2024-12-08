using System.Drawing;
using FluentAssertions;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class ComponentTests
{
    private class TestComponent : Component
    {
        public TestComponent(string name, int[] slots, int size=1) : base(name, slots, size)
        {
        }
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var component = new TestComponent("Test Component",[]);

        // Assert
        component.Name.Should().Be("Test Component");
        component.IsDestroyed.Should().BeFalse();
        component.IsActive.Should().BeTrue();
        component.IsMounted.Should().BeFalse();
    }

    [Fact]
    public void Mount_SetsIsMountedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);

        // Act
        component.Mount(new[] { 0 });

        // Assert
        component.IsMounted.Should().BeTrue();
    }

    [Fact]
    public void UnMount_ResetsMountedSlots()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);
        component.Mount(new[] { 0 });

        // Act
        component.UnMount();

        // Assert
        component.IsMounted.Should().BeFalse();
    }

    [Fact]
    public void UnMount_ThrowsExceptionForFixedComponents()
    {
        // Arrange
        var component = new TestComponent("Fixed Component", [0]);

        // Act & Assert
        var exception = Assert.Throws<ComponentException>(() => component.UnMount());
        exception.Message.Should().Be("Fixed components cannot be unmounted.");
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);

        // Act
        component.Hit();

        // Assert
        component.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Activate_DeactivateTogglesIsActive()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);
        
        // Act & Assert
        component.IsActive.Should().BeTrue(); // Default state
        
        component.Deactivate();
        component.IsActive.Should().BeFalse();
        
        component.Activate();
        component.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsMounted_ReturnsTrueWhenMountedAtSlotsNotEmpty()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        
        // Act & Assert
        component.IsMounted.Should().BeFalse(); // Initially not mounted
        
        component.Mount([0, 1]);
        component.IsMounted.Should().BeTrue(); // Mounted with slots
        
        component.UnMount();
        component.IsMounted.Should().BeFalse(); // Unmounted
    }

    [Fact]
    public void Mount_IgnoresIfAlreadyMounted()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        component.Mount([0, 1]);
        var initialSlots = component.MountedAtSlots;

        // Act
        component.Mount([2, 3]); // Try to mount again with different slots

        // Assert
        component.MountedAtSlots.Should().BeEquivalentTo(initialSlots); // Should keep original slots
    }
    
    [Fact]
    public void Mount_ComponentWithWrongSize_Throws()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        
        // Act & Assert
        var exception = Assert.Throws<ComponentException>(() => component.Mount([2]));// Try to mount 
        exception.Message.Should().Be("Component Test Component requires 2 slots.");
        
    }

    [Fact]
    public void UnMount_IgnoresIfNotMounted()
    {
        // Arrange
        var component = new TestComponent("Test Component", []);

        // Act & Assert - should not throw
        component.UnMount();
        component.IsMounted.Should().BeFalse();
    }
}
