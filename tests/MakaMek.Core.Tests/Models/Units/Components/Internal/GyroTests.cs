using Shouldly;
using Sanet.MakaMek.Core.Models.Units.Components.Internal;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Components.Internal;

public class GyroTests
{

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var gyro = new Gyro();

        // Assert
        gyro.Name.ShouldBe("Gyro");
        gyro.MountedAtSlots.ToList().Count.ShouldBe(4);
        gyro.MountedAtSlots.ShouldBe([3, 4, 5, 6]);
        gyro.IsDestroyed.ShouldBeFalse();
    }
}