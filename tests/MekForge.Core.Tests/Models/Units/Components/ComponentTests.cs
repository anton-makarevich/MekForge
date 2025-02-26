using Shouldly;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Units;
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
    
    private class TestUnitPart : UnitPart
    {
        public TestUnitPart(string name, PartLocation location, int maxArmor, int maxStructure, int slots) 
            : base(name, location, maxArmor, maxStructure, slots)
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
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);


        // Act
        component.Mount([0], unitPart);

        // Assert
        component.IsMounted.ShouldBeTrue();
    }

    [Fact]
    public void UnMount_ResetsMountedSlots()
    {
        // Arrange
        var component = new TestComponent("Test Component",[]);
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);

        component.Mount([0],unitPart);

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
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        
        // Act & Assert
        component.IsMounted.ShouldBeFalse(); // Initially not mounted
        
        component.Mount([0, 1],unitPart);
        component.IsMounted.ShouldBeTrue(); // Mounted with slots
        
        component.UnMount();
        component.IsMounted.ShouldBeFalse(); // Unmounted
    }

    [Fact]
    public void Mount_IgnoresIfAlreadyMounted()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);

        component.Mount([0, 1],unitPart);
        var initialSlots = component.MountedAtSlots;

        // Act
        component.Mount([2, 3],unitPart); // Try to mount again with different slots

        // Assert
        component.MountedAtSlots.ShouldBeEquivalentTo(initialSlots); // Should keep original slots
    }
    
    [Fact]
    public void Mount_ComponentWithWrongSize_Throws()
    {
        // Arrange
        var component = new TestComponent("Test Component", [],2);
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);

        
        // Act & Assert
        var exception = Assert.Throws<ComponentException>(() => component.Mount([2],unitPart));// Try to mount 
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
    
    #region Component Location Tests
    
    [Fact]
    public void Mount_WithUnitPart_ShouldSetMountedOnProperty()
    {
        // Arrange
        var component = new TestComponent("Test Component", [], 2);
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        
        // Act
        component.Mount([0, 1], unitPart);
        
        // Assert
        component.MountedOn.ShouldBe(unitPart);
        component.GetLocation().ShouldBe(PartLocation.LeftArm);
    }
    
    [Fact]
    public void UnMount_ShouldClearMountedOnProperty()
    {
        // Arrange
        var component = new TestComponent("Test Component", [], 2);
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        component.Mount([0, 1], unitPart);
        
        // Act
        component.UnMount();
        
        // Assert
        component.MountedOn.ShouldBeNull();
        component.GetLocation().ShouldBeNull();
    }
    
    [Fact]
    public void TryAddComponent_ShouldSetComponentLocation()
    {
        // Arrange
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        var component = new TestComponent("Test Component", [], 2);
        
        // Act
        var result = unitPart.TryAddComponent(component);
        
        // Assert
        result.ShouldBeTrue();
        component.MountedOn.ShouldBe(unitPart);
        component.GetLocation().ShouldBe(PartLocation.LeftArm);
    }
    
    [Fact]
    public void RemoveComponent_ShouldUnmountComponent()
    {
        // Arrange
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        var component = new TestComponent("Test Component", [], 2);
        unitPart.TryAddComponent(component);
        
        // Act
        var result = unitPart.RemoveComponent(component);
        
        // Assert
        result.ShouldBeTrue();
        component.IsMounted.ShouldBeFalse();
        component.MountedOn.ShouldBeNull();
        unitPart.Components.ShouldNotContain(component);
    }
    
    [Fact]
    public void RemoveComponent_ShouldReturnFalse_WhenComponentNotInPart()
    {
        // Arrange
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        var component = new TestComponent("Test Component", [], 2);
        
        // Act
        var result = unitPart.RemoveComponent(component);
        
        // Assert
        result.ShouldBeFalse();
    }
    
    [Fact]
    public void FixedComponent_ShouldHaveCorrectLocation()
    {
        // Arrange
        var unitPart = new TestUnitPart("Test Part", PartLocation.LeftArm, 10, 5, 10);
        var fixedComponent = new TestComponent("Fixed Component", [0, 1], 2);
        
        // Act
        var result = unitPart.TryAddComponent(fixedComponent);
        
        // Assert
        result.ShouldBeTrue();
        fixedComponent.MountedOn.ShouldBe(unitPart);
        fixedComponent.GetLocation().ShouldBe(PartLocation.LeftArm);
    }
    
    #endregion
}
