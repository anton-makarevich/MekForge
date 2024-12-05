using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units;

public class UnitPartTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var part = new UnitPart("Left Arm", PartLocation.LeftArm, 10, 5, 12);

        // Assert
        part.Name.Should().Be("Left Arm");
        part.Location.Should().Be(PartLocation.LeftArm);
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
        var part = new UnitPart("Test Part", PartLocation.LeftArm, maxArmor, maxStructure, 12);

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
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 0, 5, 12);
        var masc = new Masc("Test MASC", 2);
        part.TryAddComponent(masc);

        // Act
        part.ApplyDamage(10); // Ensure structure is destroyed

        // Assert
        part.IsDestroyed.Should().BeTrue();
        masc.IsDestroyed.Should().BeFalse(); // Component should not be automatically destroyed
        masc.IsActive.Should().BeFalse(); // But it should still be inactive since it was initialized that way
    }

    [Fact]
    public void GetComponents_ReturnsCorrectComponentTypes()
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5, 12);
        var masc = new Masc("Test MASC", 2);
        part.TryAddComponent(masc);

        // Act
        var mascComponents = part.GetComponents<Masc>();
        var jumpJetComponents = part.GetComponents<JumpJets>();

        // Assert
        mascComponents.Should().HaveCount(1);
        jumpJetComponents.Should().BeEmpty();
        mascComponents.First().Should().Be(masc);
    }

    [Fact]
    public void TryAddComponent_RespectsSlotLimits()
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5, 3);
        var smallComponent = new Masc("Small MASC", 2);
        var largeComponent = new Masc("Large MASC", 4);

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
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5, 3);
        var smallComponent = new Masc("Small MASC", 2);
        var largeComponent = new Masc("Large MASC", 4);

        // Act & Assert
        part.CanAddComponent(smallComponent).Should().BeTrue();
        part.CanAddComponent(largeComponent).Should().BeFalse();

        part.TryAddComponent(smallComponent);
        part.CanAddComponent(largeComponent).Should().BeFalse();
        part.CanAddComponent(new Masc("Tiny MASC", 1)).Should().BeTrue();
    }
}
