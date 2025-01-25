using Shouldly;
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
        component.Name.ShouldBe("Test Component");
        component.IsDestroyed.ShouldBeFalse();
        component.IsActive.ShouldBeTrue();
        component.IsMounted.ShouldBeFalse();
    }

    [Fact]
    public void Mount_SetsIsMountedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);

        // Act
        component.Mount(new[] { 0 });

        // Assert
        component.IsMounted.ShouldBeTrue();
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
        component.IsMounted.ShouldBeFalse();
    }

    [Fact]
    public void UnMount_ThrowsExceptionForFixedComponents()
    {
        // Arrange
        var component = new TestComponent("Fixed Component", [0]);

        // Act & Assert
        var exception = Assert.Throws<ComponentException>(() => component.UnMount());
        exception.Message.ShouldBe("Fixed components cannot be unmounted.");
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);

        // Act
        component.Hit();

        // Assert
        component.IsDestroyed.ShouldBeTrue();
        component.Hits.ShouldBe(1);
    }

    [Fact]
    public void Activate_DeactivateTogglesIsActive()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);
        
        // Act & Assert
        component.IsActive.ShouldBeTrue(); // Default state
        
        component.Deactivate();
        component.IsActive.ShouldBeFalse();
        
        component.Activate();
        component.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void IsMounted_ReturnsTrueWhenMountedAtSlotsNotEmpty()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        
        // Act & Assert
        component.IsMounted.ShouldBeFalse(); // Initially not mounted
        
        component.Mount([0, 1]);
        component.IsMounted.ShouldBeTrue(); // Mounted with slots
        
        component.UnMount();
        component.IsMounted.ShouldBeFalse(); // Unmounted
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
        component.MountedAtSlots.ShouldBeEquivalentTo(initialSlots); // Should keep original slots
    }
    
    [Fact]
    public void Mount_ComponentWithWrongSize_Throws()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        
        // Act & Assert
        var exception = Assert.Throws<ComponentException>(() => component.Mount([2]));// Try to mount 
        exception.Message.ShouldBe("Component Test Component requires 2 slots.");
        
    }

    [Fact]
    public void UnMount_IgnoresIfNotMounted()
    {
        // Arrange
        var component = new TestComponent("Test Component", []);

        // Act & Assert - should not throw
        component.UnMount();
        component.IsMounted.ShouldBeFalse();
    }
}
