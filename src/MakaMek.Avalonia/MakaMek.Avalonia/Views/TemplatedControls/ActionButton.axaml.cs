using Avalonia;
using Avalonia.Controls;

namespace Sanet.MakaMek.Avalonia.Views.TemplatedControls;

public class ActionButton : Button
{
    public static readonly StyledProperty<string> IconDataProperty = AvaloniaProperty.Register<ActionButton, string>(
        nameof(IconData));

    public string IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
}
