using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class ComponentTests
{
    private class TestComponent : Component
    {
        public TestComponent(string name, int[] slots) : base(name, slots)
        {
        }
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var component = new TestComponent("Test Component", new[] { 0, 1 });

        // Assert
        component.Name.Should().Be("Test Component");
        component.RequiredSlots.Should().BeEquivalentTo(new[] { 0, 1 });
        component.IsDestroyed.Should().BeFalse();
        component.IsActive.Should().BeTrue();
        component.IsMounted.Should().BeFalse();
    }

    [Fact]
    public void Mount_SetsIsMountedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component", new[] { 2, 3, 4 });

        // Act
        component.Mount();

        // Assert
        component.IsMounted.Should().BeTrue();
    }

    [Fact]
    public void Hit_SetsIsDestroyedToTrue()
    {
        // Arrange
        var component = new TestComponent("Test Component", new[] { 0, 1 });

        // Act
        component.Hit();

        // Assert
        component.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void Activate_DeactivateTogglesIsActive()
    {
        // Arrange
        var component = new TestComponent("Test Component", new[] { 0, 1 });
        
        // Act & Assert
        component.IsActive.Should().BeTrue(); // Default state
        
        component.Deactivate();
        component.IsActive.Should().BeFalse();
        
        component.Activate();
        component.IsActive.Should().BeTrue();
    }
}
