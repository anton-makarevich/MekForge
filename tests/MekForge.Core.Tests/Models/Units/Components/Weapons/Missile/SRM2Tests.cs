using Shouldly;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons.Missile;

public class SRM2Tests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var srm2 = new SRM2();

        // Assert
        srm2.Name.ShouldBe("SRM-2");
        srm2.Size.ShouldBe(1);
        srm2.Heat.ShouldBe(1);
        srm2.Damage.ShouldBe(4); // Total damage for all missiles
        srm2.BattleValue.ShouldBe(25);
        srm2.AmmoType.ShouldBe(AmmoType.SRM2);
        srm2.MinimumRange.ShouldBe(0);
        srm2.ShortRange.ShouldBe(3);
        srm2.MediumRange.ShouldBe(6);
        srm2.LongRange.ShouldBe(9);
        srm2.IsDestroyed.ShouldBeFalse();
    }

    [Fact]
    public void Hit_DestroysSRM2()
    {
        // Arrange
        var srm2 = new SRM2();

        // Act
        srm2.Hit();

        // Assert
        srm2.IsDestroyed.ShouldBeTrue();
    }
}
