using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Internal;

public class GyroTests
{

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var gyro = new Gyro();

        // Assert
        gyro.Name.Should().Be("Gyro");
        gyro.MountedAtSlots.Should().HaveCount(4);
        gyro.MountedAtSlots.Should().ContainInOrder(3, 4, 5, 6);
        gyro.IsDestroyed.Should().BeFalse();
    }
}