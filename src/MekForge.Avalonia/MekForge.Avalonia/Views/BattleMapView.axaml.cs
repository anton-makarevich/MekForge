using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Sanet.MekForge.Avalonia.Controls;
using Sanet.MekForge.Avalonia.ViewModels;
using Sanet.MekForge.Core.Models;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MekForge.Avalonia.Views;

public partial class BattleMapView : BaseView<BattleMapViewModel>
{
    private Point _lastPointerPosition;
    private TranslateTransform _mapTransform = new TranslateTransform();

    public BattleMapView()
    {
        InitializeComponent();
        MapCanvas.RenderTransform = _mapTransform;
        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        
    }

    private void RenderMap(BattleMap battleMap)
    {
        MapCanvas.Children.Clear();

        foreach (var hex in battleMap.GetHexes())
        {
            var hexControl = new HexControl();
            hexControl.SetHex(hex);

            Canvas.SetLeft(hexControl, hex.Coordinates.X);
            Canvas.SetTop(hexControl, hex.Coordinates.Y);

            MapCanvas.Children.Add(hexControl);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (MapCanvas.Children.Count == 0)
        {
            if (ViewModel != null) RenderMap(ViewModel.BattleMap);
            return;
        }
        _lastPointerPosition = e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(this);
            var delta = position - _lastPointerPosition;
            _lastPointerPosition = position;

            _mapTransform.X += delta.X;
            _mapTransform.Y += delta.Y;
        }
    }
}
