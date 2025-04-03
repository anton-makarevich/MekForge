using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Sanet.MakaMek.Avalonia.Views.NewGame.Fragments;

public partial class NetworkFragment : UserControl
{
    public NetworkFragment()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
