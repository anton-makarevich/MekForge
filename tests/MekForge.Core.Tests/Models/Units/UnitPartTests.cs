using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units;

public class UnitPartTests
{
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
        var part = new TestUnitPart("Left Arm", PartLocation.LeftSide, 10, 5, 12);

        // Assert
        part.Name.Should().Be("Left Arm");
        part.Location.Should().Be(PartLocation.LeftSide);
        part.MaxArmor.Should().Be(10);
        part.CurrentArmor.Should().Be(10);
        part.MaxStructure.Should().Be(5);
        part.CurrentStructure.Should().Be(5);
        part.TotalSlots.Should().Be(12);
        part.UsedSlots.Should().Be(0);
        part.AvailableSlots.Should().Be(12);
        part.Components.Should().BeEmpty();
        part.IsDestroyed.Should().BeFalse();
    }

    [Theory]
    [InlineData(5, 10, 5, 0)] // Damage less than armor
    [InlineData(14, 10, 5, 0)] // Damage less than armor + structure
    [InlineData(15, 10, 5, 0)] // Damage equals armor + structure
    [InlineData(20, 10, 5, 5)] // Damage exceeds armor + structure
    public void ApplyDamage_HandlesVariousDamageScenarios(int damage, int maxArmor, int maxStructure, int expectedExcess)
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, maxArmor, maxStructure, 12);

        // Act
        var excessDamage = part.ApplyDamage(damage);

        // Assert
        excessDamage.Should().Be(expectedExcess);
        
        if (damage <= maxArmor)
        {
            part.CurrentArmor.Should().Be(maxArmor - damage);
            part.CurrentStructure.Should().Be(maxStructure);
            part.IsDestroyed.Should().BeFalse();
        }
        else if (damage < maxArmor + maxStructure)
        {
            part.CurrentArmor.Should().Be(0);
            part.CurrentStructure.Should().Be(maxStructure - (damage - maxArmor));
            part.IsDestroyed.Should().BeFalse();
        }
        else
        {
            part.CurrentArmor.Should().Be(0);
            part.CurrentStructure.Should().Be(0);
            part.IsDestroyed.Should().BeTrue();
        }
    }

    [Fact]
    public void ApplyDamage_DoesNotDestroyComponentsWhenStructureIsDestroyed()
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, 0, 5, 12);
        var masc = new TestComponent("Test MASC", new[] { 0, 1 });
        part.TryAddComponent(masc);

        // Act
        part.ApplyDamage(10); // Ensure structure is destroyed

        // Assert
        part.IsDestroyed.Should().BeTrue();
        masc.IsDestroyed.Should().BeFalse(); // Component should not be automatically destroyed
        masc.IsActive.Should().BeTrue(); // Components start active by default
    }

    [Fact]
    public void GetComponents_ReturnsCorrectComponentTypes()
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, 10, 5, 12);
        var testComponent = new TestComponent("Test Component", new[] { 0, 1 });
        part.TryAddComponent(testComponent);

        // Act
        var testComponents = part.GetComponents<TestComponent>();
        var jumpJetComponents = part.GetComponents<JumpJets>();

        // Assert
        testComponents.Should().HaveCount(1);
        jumpJetComponents.Should().BeEmpty();
        testComponents.First().Should().Be(testComponent);
    }

    [Fact]
    public void TryAddComponent_RespectsSlotLimits()
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, 10, 5, 3);
        var smallComponent = new TestComponent("Small Component", new[] { 0, 1 });
        var largeComponent = new TestComponent("Large Component", new[] { 0, 1, 2, 3 });

        // Act & Assert
        part.TryAddComponent(smallComponent).Should().BeTrue();
        part.UsedSlots.Should().Be(2);
        part.AvailableSlots.Should().Be(1);
        
        part.TryAddComponent(largeComponent).Should().BeFalse();
        part.Components.Should().HaveCount(1);
        part.UsedSlots.Should().Be(2);
    }

    [Fact]
    public void CanAddComponent_ChecksSlotAvailability()
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, 10, 5, 3);
        var smallComponent = new TestComponent("Small Component", new[] { 0, 1 });
        var largeComponent = new TestComponent("Large Component", new[] { 0, 1, 2, 3 });

        // Act & Assert
        part.CanAddComponent(smallComponent).Should().BeTrue();
        part.CanAddComponent(largeComponent).Should().BeFalse();

        part.TryAddComponent(smallComponent);
        part.CanAddComponent(largeComponent).Should().BeFalse();
        part.CanAddComponent(new TestComponent("Tiny Component", new[] { 2 })).Should().BeTrue();
    }

    [Fact]
    public void GetComponentAtSlot_ReturnsCorrectComponent()
    {
        // Arrange
        var part = new TestUnitPart("Test Part", PartLocation.LeftSide, 10, 5, 6);
        var component1 = new TestComponent("Component 1", new[] { 0, 1 });
        var component2 = new TestComponent("Component 2", new[] { 3, 4, 5 });
        
        part.TryAddComponent(component1);
        part.TryAddComponent(component2);

        // Act & Assert
        part.GetComponentAtSlot(0).Should().Be(component1);
        part.GetComponentAtSlot(1).Should().Be(component1);
        part.GetComponentAtSlot(2).Should().BeNull();
        part.GetComponentAtSlot(3).Should().Be(component2);
        part.GetComponentAtSlot(4).Should().Be(component2);
        part.GetComponentAtSlot(5).Should().Be(component2);
    }

    private class TestComponent : Component
    {
        public TestComponent(string name, int[] slots) : base(name, slots)
        {
        }
    }
}
