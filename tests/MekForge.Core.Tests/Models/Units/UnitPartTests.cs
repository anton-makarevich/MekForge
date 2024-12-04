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
        var part = new UnitPart("Left Arm", PartLocation.LeftArm, 10, 5);

        // Assert
        Assert.Equal("Left Arm", part.Name);
        Assert.Equal(PartLocation.LeftArm, part.Location);
        Assert.Equal(10, part.MaxArmor);
        Assert.Equal(10, part.CurrentArmor);
        Assert.Equal(5, part.MaxStructure);
        Assert.Equal(5, part.CurrentStructure);
        Assert.Empty(part.Components);
    }

    [Theory]
    [InlineData(5, 10, 5, 0)] // Damage less than armor
    [InlineData(15, 10, 5, 0)] // Damage equals armor + structure
    [InlineData(20, 10, 5, 5)] // Damage exceeds armor + structure
    public void ApplyDamage_HandlesVariousDamageScenarios(int damage, int maxArmor, int maxStructure, int expectedExcess)
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, maxArmor, maxStructure);

        // Act
        var excessDamage = part.ApplyDamage(damage);

        // Assert
        Assert.Equal(expectedExcess, excessDamage);
        
        if (damage <= maxArmor)
        {
            Assert.Equal(maxArmor - damage, part.CurrentArmor);
            Assert.Equal(maxStructure, part.CurrentStructure);
        }
        else if (damage <= maxArmor + maxStructure)
        {
            Assert.Equal(0, part.CurrentArmor);
            Assert.Equal(maxStructure - (damage - maxArmor), part.CurrentStructure);
        }
        else
        {
            Assert.Equal(0, part.CurrentArmor);
            Assert.Equal(0, part.CurrentStructure);
        }
    }

    [Fact]
    public void GetComponents_ReturnsCorrectComponentTypes()
    {
        // Arrange
        var part = new UnitPart("Test Part", PartLocation.LeftArm, 10, 5);
        var masc = new Masc("Test MASC", 2);
        part.Components.Add(masc);

        // Act
        var mascComponents = part.GetComponents<Masc>();
        var jumpJetComponents = part.GetComponents<JumpJets>();

        // Assert
        Assert.Single(mascComponents);
        Assert.Empty(jumpJetComponents);
        Assert.Equal(masc, mascComponents.First());
    }
}
