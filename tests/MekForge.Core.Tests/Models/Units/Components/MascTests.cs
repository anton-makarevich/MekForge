using Sanet.MekForge.Core.Models.Units.Components;
using Xunit;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components;

public class MascTests
{
    [Fact]
    public void Constructor_SetsNameAndSlots()
    {
        // Arrange & Act
        var masc = new Masc("Test MASC", 2);

        // Assert
        Assert.Equal("Test MASC", masc.Name);
        Assert.Equal(2, masc.Slots);
        Assert.False(masc.IsDestroyed);
        Assert.False(masc.IsActive);
    }

    [Fact]
    public void ApplyDamage_DestroysAndDeactivatesComponent()
    {
        // Arrange
        var masc = new Masc("Test MASC", 2);

        // Act
        masc.ApplyDamage();

        // Assert
        Assert.True(masc.IsDestroyed);
        Assert.False(masc.IsActive);
    }
}
