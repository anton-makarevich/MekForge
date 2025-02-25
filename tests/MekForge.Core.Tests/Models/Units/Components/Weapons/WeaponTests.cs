using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;
using Shouldly;

namespace Sanet.MekForge.Core.Tests.Models.Units.Components.Weapons;

public class WeaponTests
{
    private readonly Weapon _weapon;

    public WeaponTests()
    {
        _weapon = new LRM5();
    }

    [Theory]
    [InlineData(0, WeaponRange.OutOfRange)] // Attacker's position
    [InlineData(6, WeaponRange.Minimum)] // At minimum range
    [InlineData(7, WeaponRange.Short)] // At short range boundary
    [InlineData(10, WeaponRange.Medium)] // Within short range
    [InlineData(14, WeaponRange.Medium)] // At medium range boundary
    [InlineData(17, WeaponRange.Long)] // Within long range
    [InlineData(21, WeaponRange.Long)] // At long range boundary
    [InlineData(22, WeaponRange.OutOfRange)] // Beyond long range
    public void GetRangeBracket_ReturnsCorrectRange(int distance, WeaponRange expectedRange)
    {
        // Act
        var result = _weapon.GetRangeBracket(distance);

        // Assert
        result.ShouldBe(expectedRange);
    }
}
