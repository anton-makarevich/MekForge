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
        Assert.Equal("Left Arm", part.Name);
        Assert.Equal(PartLocation.LeftArm, part.Location);
        Assert.Equal(10, part.MaxArmor);
        Assert.Equal(10, part.CurrentArmor);
        Assert.Equal(5, part.MaxStructure);
        Assert.Equal(5, part.CurrentStructure);
        Assert.Equal(12, part.TotalSlots);
        Assert.Equal(0, part.UsedSlots);
        Assert.Equal(12, part.AvailableSlots);
        Assert.Empty(part.Components);
        Assert.False(part.IsDestroyed);
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
        Assert.Equal(expectedExcess, excessDamage);
        
        if (damage <= maxArmor)
        {
            Assert.Equal(maxArmor - damage, part.CurrentArmor);
            Assert.Equal(maxStructure, part.CurrentStructure);
            Assert.False(part.IsDestroyed);
        }
        else if (damage < maxArmor + maxStructure)
        {
            Assert.Equal(0, part.CurrentArmor);
            Assert.Equal(maxStructure - (damage - maxArmor), part.CurrentStructure);
            Assert.False(part.IsDestroyed);
        }
        else
        {
            Assert.Equal(0, part.CurrentArmor);
            Assert.Equal(0, part.CurrentStructure);
            Assert.True(part.IsDestroyed);
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
        Assert.True(part.IsDestroyed);
        Assert.False(masc.IsDestroyed); // Component should not be automatically destroyed
        Assert.False(masc.IsActive); // But it should still be inactive since it was initialized that way
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
        Assert.Single(mascComponents);
        Assert.Empty(jumpJetComponents);
        Assert.Equal(masc, mascComponents.First());
    }

    [Fact]
    public void TryAddComponent_RespectsSlotLimits()
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5, 3);
        var smallComponent = new Masc("Small MASC", 2);
        var largeComponent = new Masc("Large MASC", 4);

        // Act & Assert
        Assert.True(part.TryAddComponent(smallComponent));
        Assert.Equal(2, part.UsedSlots);
        Assert.Equal(1, part.AvailableSlots);
        
        Assert.False(part.TryAddComponent(largeComponent));
        Assert.Single(part.Components);
        Assert.Equal(2, part.UsedSlots);
    }

    [Fact]
    public void CanAddComponent_ChecksSlotAvailability()
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5, 3);
        var smallComponent = new Masc("Small MASC", 2);
        var largeComponent = new Masc("Large MASC", 4);

        // Act & Assert
        Assert.True(part.CanAddComponent(smallComponent));
        Assert.False(part.CanAddComponent(largeComponent));

        part.TryAddComponent(smallComponent);
        Assert.False(part.CanAddComponent(largeComponent));
        Assert.True(part.CanAddComponent(new Masc("Tiny MASC", 1)));
    }
}
