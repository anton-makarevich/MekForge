using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Sanet.MekForge.Avalonia.Converters;

public class HexColorToBrushConverter : IValueConverter
{
    public object Convert(
        object? value, Type targetType, 
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is not string hexColor) return new SolidColorBrush(Colors.Transparent);
        try
        {
            var color = Color.Parse(hexColor);
            return new SolidColorBrush(color);
        }
        catch
        {
            // Return transparent if parsing fails
            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
