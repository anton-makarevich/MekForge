using Avalonia.Controls;
using Sanet.MVVM.Core.Views;

namespace Sanet.MekForge.Avalonia.Views;

public partial class BattleMapView : UserControl, IBaseView
{
    public BattleMapView()
    {
        InitializeComponent();
    }

    public object? ViewModel { get; set; }
}
