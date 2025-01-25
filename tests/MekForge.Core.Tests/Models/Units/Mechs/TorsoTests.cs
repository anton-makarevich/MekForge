using Shouldly;
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
    [InlineData(9, 10, 3, 5, 1)] // Rear damage exceeds rear armor and depletes structure
    public void ApplyDamage_HandlesVariousDamageScenarios(int damage, int maxArmor, int maxRearArmor, int maxStructure, int expectedExcess)
    {
        // Arrange
        var torso = new TestTorso("Test Torso", PartLocation.LeftTorso, maxArmor, maxRearArmor, maxStructure);
        var direction = damage == 3 || damage == 9 ? HitDirection.Rear : HitDirection.Front;

        // Act
        var excessDamage = torso.ApplyDamage(damage, direction);

        // Assert
        excessDamage.ShouldBe(expectedExcess);

        if (direction == HitDirection.Front)
        {
            if (damage <= maxArmor)
            {
                torso.CurrentArmor.ShouldBe(maxArmor - damage);
                torso.CurrentRearArmor.ShouldBe(maxRearArmor);
                torso.CurrentStructure.ShouldBe(maxStructure);
            }
            else
            {
                torso.CurrentArmor.ShouldBe(0);
                var remainingDamage = damage - maxArmor;
                if (remainingDamage < maxStructure)
                {
                    torso.CurrentStructure.ShouldBe(maxStructure - remainingDamage);
                }
                else
                {
                    torso.CurrentStructure.ShouldBe(0);
                }
            }
        }
        else // Rear
        {
            if (damage <= maxRearArmor)
            {
                torso.CurrentRearArmor.ShouldBe(maxRearArmor - damage);
                torso.CurrentArmor.ShouldBe(maxArmor);
                torso.CurrentStructure.ShouldBe(maxStructure);
            }
            else
            {
                torso.CurrentRearArmor.ShouldBe(0);
                var remainingDamage = damage - maxRearArmor;
                if (remainingDamage < maxStructure)
                {
                    torso.CurrentStructure.ShouldBe(maxStructure - remainingDamage);
                }
                else
                {
                    torso.CurrentStructure.ShouldBe(0);
                }
            }
        }
    }

    [Theory]
    [InlineData(5, 10, 5, 5, 0)] // Damage does not exceed rear armor
    [InlineData(15, 10, 5, 5,5)] // Damage exceeds rear armor
    [InlineData(8, 10, 5, 5,0)] // Damage exceeds rear armor but structure remains
    public void ApplyDamage_HandlesRearArmor(int damage, int maxArmor, int maxRearArmor, int maxStructure, int expectedExcess)
    {
        // Arrange
        var torso = new TestTorso("Test Torso", PartLocation.CenterTorso, maxArmor, maxRearArmor, maxStructure);

        // Act
        var excessDamage = torso.ApplyDamage(damage, HitDirection.Rear);

        // Assert
        excessDamage.ShouldBe(expectedExcess);

        if (damage <= maxRearArmor)
        {
            torso.CurrentRearArmor.ShouldBe(maxRearArmor - damage);
            torso.CurrentStructure.ShouldBe(maxStructure);
        }
        else if (damage < maxArmor + maxStructure)
        {
            torso.CurrentRearArmor.ShouldBe(0);
            torso.CurrentStructure.ShouldBe(maxStructure - (damage - maxRearArmor));
        }
    }
}