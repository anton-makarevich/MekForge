using System;
using Avalonia.Data.Converters;
using Sanet.MekForge.Core.Models.Units.Mechs;

namespace Sanet.MekForge.Avalonia.Converters;

public class TorsoPropertyConverter : IValueConverter
{
    public object? Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is Torso torso && parameter is string propertyName)
        {
            return propertyName switch
            {
                "CurrentRearArmor" => torso.CurrentRearArmor,
                "MaxRearArmor" => torso.MaxRearArmor,
                _ => 0
            };
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
