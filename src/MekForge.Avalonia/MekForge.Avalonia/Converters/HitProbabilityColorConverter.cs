using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Sanet.MekForge.Avalonia.Converters;

/// <summary>
/// Converts a hit probability value to an appropriate color
/// </summary>
public class HitProbabilityColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a hit probability value to a color
    /// </summary>
    /// <param name="value">Hit probability as a double (0-100, or -1 for N/A)</param>
    /// <param name="targetType">Target type (should be IBrush)</param>
    /// <param name="parameter">Optional threshold values as comma-separated string "lowThreshold,mediumThreshold" (default: "30,60")</param>
    /// <param name="culture">Culture info</param>
    /// <returns>Color brush based on the probability value</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double probability || !targetType.IsAssignableTo(typeof(IBrush)))
            return new SolidColorBrush(Colors.White);

        // Handle N/A case (probability < 0)
        if (probability < 0)
            return new SolidColorBrush(Colors.Gray);

        // Get threshold values (default: low < 30%, medium < 60%, high >= 60%)
        var lowThreshold = 30;
        var mediumThreshold = 60;

        if (parameter is string thresholds)
        {
            var parts = thresholds.Split(',');
            if (parts.Length >= 2)
            {
                int.TryParse(parts[0], out lowThreshold);
                int.TryParse(parts[1], out mediumThreshold);
            }
        }

        // Return appropriate color based on probability
        if (probability < lowThreshold)
            return new SolidColorBrush(Colors.Red);
        if (probability < mediumThreshold)
            return new SolidColorBrush(Colors.Orange);
        return new SolidColorBrush(Colors.Green);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
