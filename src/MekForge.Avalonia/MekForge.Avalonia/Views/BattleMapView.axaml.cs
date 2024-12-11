using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MekForge.Avalonia.Controls;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Services;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MekForge.Avalonia.Views;

public partial class BattleMapView : BaseView<BattleMapViewModel>
{
    private Point _lastPointerPosition;
    private readonly TranslateTransform _mapTranslateTransform = new();
    private readonly ScaleTransform _mapScaleTransform = new() { ScaleX = 1, ScaleY = 1 };
    private const double MinScale = 0.5;
    private const double MaxScale = 2.0;
    private const double ScaleStep = 0.1;

    public BattleMapView()
    {
        InitializeComponent();
        
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_mapScaleTransform);
        transformGroup.Children.Add(_mapTranslateTransform);
        MapCanvas.RenderTransform = transformGroup;
        
        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        this.PointerWheelChanged += OnPointerWheelChanged;
        
        var pinchGestureRecognizer = new PinchGestureRecognizer();
        MapCanvas.GestureRecognizers.Add(pinchGestureRecognizer);
        MapCanvas.AddHandler(Gestures.PinchEvent, OnPinchChanged);
    }

    private void RenderMap(BattleMap battleMap, IImageService<Bitmap> imageService)
    {
        MapCanvas.Children.Clear();

        foreach (var hex in battleMap.GetHexes())
        {
            var hexControl = new HexControl(hex, imageService);
            MapCanvas.Children.Add(hexControl);
        }

        if (ViewModel?.Unit == null) return;
        var unitControl = new UnitControl(ViewModel.Unit, imageService);
        MapCanvas.Children.Add(unitControl);
        var hexToDeploy = battleMap.GetHexes().FirstOrDefault();
        if (hexToDeploy == null) return;
        ViewModel.Unit.Deploy(hexToDeploy.Coordinates);
        unitControl.Update();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastPointerPosition = e.GetPosition(this);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        var position = e.GetPosition(this);
        var delta = position - _lastPointerPosition;
        _lastPointerPosition = position;

        _mapTranslateTransform.X += delta.X;
        _mapTranslateTransform.Y += delta.Y;
    }
    
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var delta = e.Delta.Y * ScaleStep;
        var newScale = _mapScaleTransform.ScaleX + delta;
        
        if (newScale >= MinScale && newScale <= MaxScale)
        {
            _mapScaleTransform.ScaleX = newScale;
            _mapScaleTransform.ScaleY = newScale;
        }
    }
    
    private void OnPinchChanged(object? sender, PinchEventArgs e)
    {
        var newScale = _mapScaleTransform.ScaleX * e.Scale;

        if (!(newScale >= MinScale) || !(newScale <= MaxScale)) return;
        _mapScaleTransform.ScaleX = newScale;
        _mapScaleTransform.ScaleY = newScale;
    }

    protected override void OnViewModelSet()
    {
        base.OnViewModelSet();
        if (ViewModel != null && ViewModel.BattleMap != null)
        {
                RenderMap(ViewModel.BattleMap, (IImageService<Bitmap>)ViewModel.ImageService);
        }
    }
}
