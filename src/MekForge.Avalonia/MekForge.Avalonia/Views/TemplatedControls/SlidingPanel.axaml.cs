using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Sanet.MekForge.Avalonia.Views.TemplatedControls;

public class SlidingPanel : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<SlidingPanel, string>(
        nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<ICommand> CloseCommandProperty = AvaloniaProperty.Register<SlidingPanel, ICommand>(
        nameof(CloseCommand));

    public ICommand CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}
