using Shouldly;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units;

public class UnitComponentLocationTests
{
    private class TestComponent : Component
    {
        public TestComponent(string name, int size = 1) : base(name, [], size)
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
    
    private class TestUnit : Unit
    {
        public TestUnit(string chassis, string model, int tonnage, int walkMp, IEnumerable<UnitPart> parts) 
            : base(chassis, model, tonnage, walkMp, parts)
        {
        }

        public override int CalculateBattleValue() => 0;

        public override bool CanMoveBackward(MovementType type) => true;

        protected override PartLocation? GetTransferLocation(PartLocation location) => null;
    }
    
    [Fact]
    public void GetComponentsAtLocation_ShouldReturnComponentsAtSpecifiedLocation()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var rightArmPart = new TestUnitPart("Right Arm", PartLocation.RightArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, new[] { leftArmPart, rightArmPart });
        
        var leftArmComponent1 = new TestComponent("Left Arm Component 1", 2);
        var leftArmComponent2 = new TestComponent("Left Arm Component 2", 2);
        var rightArmComponent = new TestComponent("Right Arm Component", 2);
        
        leftArmPart.TryAddComponent(leftArmComponent1);
        leftArmPart.TryAddComponent(leftArmComponent2);
        rightArmPart.TryAddComponent(rightArmComponent);
        
        // Act
        var leftArmComponents = testUnit.GetComponentsAtLocation(PartLocation.LeftArm).ToList();
        var rightArmComponents = testUnit.GetComponentsAtLocation(PartLocation.RightArm).ToList();
        var headComponents = testUnit.GetComponentsAtLocation(PartLocation.Head).ToList();
        
        // Assert
        leftArmComponents.Count.ShouldBe(2);
        leftArmComponents.ShouldContain(leftArmComponent1);
        leftArmComponents.ShouldContain(leftArmComponent2);
        
        rightArmComponents.Count.ShouldBe(1);
        rightArmComponents.ShouldContain(rightArmComponent);
        
        headComponents.ShouldBeEmpty();
    }
    
    [Fact]
    public void GetComponentsAtLocation_Generic_ShouldReturnComponentsOfSpecificType()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, new[] { leftArmPart });
        
        var component1 = new TestComponent("Component 1", 2);
        var component2 = new TestDerivedComponent("Component 2", 2);
        
        leftArmPart.TryAddComponent(component1);
        leftArmPart.TryAddComponent(component2);
        
        // Act
        var allComponents = testUnit.GetComponentsAtLocation(PartLocation.LeftArm).ToList();
        var derivedComponents = testUnit.GetComponentsAtLocation<TestDerivedComponent>(PartLocation.LeftArm).ToList();
        
        // Assert
        allComponents.Count.ShouldBe(2);
        derivedComponents.Count.ShouldBe(1);
        derivedComponents.ShouldContain(component2);
    }
    
    [Fact]
    public void FindComponentPart_ShouldReturnCorrectPart()
    {
        // Arrange
        var leftArmPart = new TestUnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 10);
        var rightArmPart = new TestUnitPart("Right Arm", PartLocation.RightArm, 10, 5, 10);
        var testUnit = new TestUnit("Test", "Unit", 20, 4, new[] { leftArmPart, rightArmPart });
        
        var leftArmComponent = new TestComponent("Left Arm Component", 2);
        var rightArmComponent = new TestComponent("Right Arm Component", 2);
        var unmountedComponent = new TestComponent("Unmounted Component", 2);
        
        leftArmPart.TryAddComponent(leftArmComponent);
        rightArmPart.TryAddComponent(rightArmComponent);
        
        // Act
        var leftArmComponentPart = testUnit.FindComponentPart(leftArmComponent);
        var rightArmComponentPart = testUnit.FindComponentPart(rightArmComponent);
        var unmountedComponentPart = testUnit.FindComponentPart(unmountedComponent);
        
        // Assert
        leftArmComponentPart.ShouldBe(leftArmPart);
        rightArmComponentPart.ShouldBe(rightArmPart);
        unmountedComponentPart.ShouldBeNull();
    }
    
    // Helper class for testing generic methods
    private class TestDerivedComponent : TestComponent
    {
        public TestDerivedComponent(string name, int size = 1) : base(name, size)
        {
        }
    }
}
