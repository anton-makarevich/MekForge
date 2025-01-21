using Avalonia;
using Avalonia.Controls;

namespace Sanet.MekForge.Avalonia.Views.TemplatedControls;

public class CircleButton : Button
{
    public static readonly StyledProperty<string> IconDataProperty = AvaloniaProperty.Register<CircleButton, string>(
        nameof(IconData));

    public string IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
}
