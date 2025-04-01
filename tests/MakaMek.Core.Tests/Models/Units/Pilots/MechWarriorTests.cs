using Sanet.MakaMek.Core.Models.Units.Pilots;
using Shouldly;

namespace Sanet.MakaMek.Core.Tests.Models.Units.Pilots;

public class MechWarriorTests
{
    [Fact]
    public void Constructor_WithDefaultValues_SetsDefaultSkills()
    {
        // Arrange & Act
        var pilot = new MechWarrior("John", "Doe");

        // Assert
        pilot.FirstName.ShouldBe("John");
        pilot.LastName.ShouldBe("Doe");
        pilot.Gunnery.ShouldBe(MechWarrior.DefaultGunnery);
        pilot.Piloting.ShouldBe(MechWarrior.DefaultPiloting);
        pilot.Health.ShouldBe(MechWarrior.DefaultHealth);
    }

    [Fact]
    public void Constructor_WithCustomSkills_SetsProvidedValues()
    {
        // Arrange & Act
        var pilot = new MechWarrior("John", "Doe", gunnery: 3, piloting: 4);

        // Assert
        pilot.FirstName.ShouldBe("John");
        pilot.LastName.ShouldBe("Doe");
        pilot.Gunnery.ShouldBe(3);
        pilot.Piloting.ShouldBe(4);
        pilot.Health.ShouldBe(MechWarrior.DefaultHealth);
    }
}
