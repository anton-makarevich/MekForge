using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Sanet.MekForge.Core.Models;

namespace Sanet.MekForge.Avalonia.Controls;

public partial class HexControl : UserControl
{
    private Polygon _hexPolygon = null!;

    public HexControl()
    {
        InitializeComponent();
        _hexPolygon = this.FindControl<Polygon>("HexPolygon");
    }

    public void SetHex(Hex hex)
    {
        // TODO: Update hex appearance based on terrain and elevation
    }
}
