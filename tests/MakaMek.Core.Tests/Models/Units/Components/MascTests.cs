using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components;

public class MascTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var masc = new Masc("MASC");

        // Assert
        masc.Name.ShouldBe("MASC");
        masc.Size.ShouldBe(1);
        masc.IsDestroyed.ShouldBeFalse();
        masc.IsActive.ShouldBeFalse(); // MASC starts deactivated
    }

    [Fact]
    public void Hit_DestroysAndDeactivatesComponent()
    {
        // Arrange
        var masc = new Masc("MASC");
        masc.Activate();

        // Act
        masc.Hit();

        // Assert
        masc.IsDestroyed.ShouldBeTrue();
        masc.IsActive.ShouldBeFalse();
    }
}
