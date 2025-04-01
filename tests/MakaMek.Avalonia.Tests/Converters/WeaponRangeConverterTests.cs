using System.Globalization;
using Sanet.MakaMek.Avalonia.Converters;
using Sanet.MakaMek.Core.Models.Units.Components.Weapons;
using Shouldly;
using MakaMek.Avalonia.Tests.TestHelpers;

namespace MakaMek.Avalonia.Tests.Converters
{
    public class WeaponRangeConverterTests
    {
        private readonly WeaponRangeConverter _converter;

        public WeaponRangeConverterTests()
        {
            _converter = new WeaponRangeConverter();
        }

        [Theory]
        [InlineData(0, 6, 12, 18, "-|6|12|18")]  // No minimum range
        [InlineData(2, 6, 12, 18, "2|6|12|18")]  // With minimum range
        public void Convert_ValidWeapon_ReturnsFormattedRangeString(
            int minRange, int shortRange, int mediumRange, int longRange, string expected)
        {
            // Arrange
            var weapon = new TestWeapon(
                minimumRange: minRange,
                shortRange: shortRange,
                mediumRange: mediumRange,
                longRange: longRange);

            // Act
            var result = _converter.Convert(weapon, typeof(string), null, CultureInfo.InvariantCulture) as string;

            // Assert
            result.ShouldBe(expected);
        }

        [Fact]
        public void Convert_NullWeapon_ReturnsNull()
        {
            // Act
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Convert_NonWeaponObject_ReturnsNull()
        {
            // Act
            var result = _converter.Convert("not a weapon", typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Should.Throw<NotImplementedException>(() =>
                _converter.ConvertBack(null, typeof(Weapon), null, CultureInfo.InvariantCulture));
        }
    }
}
