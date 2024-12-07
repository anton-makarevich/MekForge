using FluentAssertions;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Core.Tests.Models.Units.Mechs;

public class TorsoTests
{
    private class TestTorso : Torso
    {
        public TestTorso(string name, PartLocation location, int maxArmor, int maxRearArmor, int maxStructure) 
            : base(name, location, maxArmor, maxRearArmor, maxStructure)
        {
        }
    }

    [Theory]
    [InlineData(5, 10, 3, 5, 0)] // Front damage less than armor
    [InlineData(3, 10, 3, 5, 0)] // Rear damage less than rear armor
    [InlineData(14, 10, 3, 5, 0)] // Front damage depletes armor and some structure
    [InlineData(4, 10, 3, 5, 1)] // Rear damage exceeds rear armor
    public void ApplyDamage_HandlesVariousDamageScenarios(int damage, int maxArmor, int maxRearArmor, int maxStructure, int expectedExcess)
    {
        // Arrange
        var torso = new TestTorso("Test Torso", PartLocation.LeftSide, maxArmor, maxRearArmor, maxStructure);
        var direction = damage == 3 || damage == 4 ? HitDirection.Rear : HitDirection.Front;

        // Act
        var excessDamage = torso.ApplyDamage(damage, direction);

        // Assert
        excessDamage.Should().Be(expectedExcess);

        if (direction == HitDirection.Front)
        {
            if (damage <= maxArmor)
            {
                torso.CurrentArmor.Should().Be(maxArmor - damage);
                torso.CurrentRearArmor.Should().Be(maxRearArmor);
                torso.CurrentStructure.Should().Be(maxStructure);
            }
            else
            {
                torso.CurrentArmor.Should().Be(0);
                var remainingDamage = damage - maxArmor;
                if (remainingDamage < maxStructure)
                {
                    torso.CurrentStructure.Should().Be(maxStructure - remainingDamage);
                }
                else
                {
                    torso.CurrentStructure.Should().Be(0);
                }
            }
        }
        else // Rear
        {
            if (damage <= maxRearArmor)
            {
                torso.CurrentRearArmor.Should().Be(maxRearArmor - damage);
                torso.CurrentArmor.Should().Be(maxArmor);
                torso.CurrentStructure.Should().Be(maxStructure);
            }
            else
            {
                torso.CurrentRearArmor.Should().Be(0);
                var remainingDamage = damage - maxRearArmor;
                if (remainingDamage < maxStructure)
                {
                    torso.CurrentStructure.Should().Be(maxStructure - remainingDamage);
                }
                else
                {
                    torso.CurrentStructure.Should().Be(0);
                }
            }
        }
    }
}