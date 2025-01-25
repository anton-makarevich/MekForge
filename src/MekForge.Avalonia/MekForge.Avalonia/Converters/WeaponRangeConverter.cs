using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;

namespace Sanet.MekForge.Avalonia.Converters
{
    public class WeaponRangeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Weapon weapon)
                return null;

            var minRange = weapon.MinimumRange == 0 ? "-" : weapon.MinimumRange.ToString();
            return $"{minRange}|{weapon.ShortRange}|{weapon.MediumRange}|{weapon.LongRange}";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
