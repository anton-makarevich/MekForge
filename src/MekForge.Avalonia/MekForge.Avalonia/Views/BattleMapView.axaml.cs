using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MekForge.Avalonia.Controls;
using Sanet.MekForge.Core.Models.Game;
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
    private const int SelectionThresholdMilliseconds = 250; // Time to distinguish selection vs pan
    private bool _isManipulating;
    private bool _isPressed;
    private CancellationTokenSource _manipulationTokenSource;

    public BattleMapView()
    {
        InitializeComponent();
        
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(_mapScaleTransform);
        transformGroup.Children.Add(_mapTranslateTransform);
        MapCanvas.RenderTransform = transformGroup;
        
        MapCanvas.PointerPressed += OnPointerPressed;
        MapCanvas.PointerMoved += OnPointerMoved;
        MapCanvas.PointerReleased += OnPointerReleased;
        MapCanvas.PointerWheelChanged += OnPointerWheelChanged;
        
        var pinchGestureRecognizer = new PinchGestureRecognizer();
        MapCanvas.GestureRecognizers.Add(pinchGestureRecognizer);
        MapCanvas.AddHandler(Gestures.PinchEvent, OnPinchChanged);
    }

    private void RenderMap(IGame game, IImageService<Bitmap> imageService)
    {
        MapCanvas.Children.Clear();

        foreach (var hex in game.GetHexes())
        {
            var hexControl = new HexControl(hex, imageService);
            MapCanvas.Children.Add(hexControl);
        }

        foreach (var unit in ViewModel.Units)
        {
            var unitControl = new UnitControl(unit, (IImageService<Bitmap>)ViewModel.ImageService);
            MapCanvas.Children.Add(unitControl);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastPointerPosition = e.GetPosition(this);
        
        _isManipulating = false; // Reset manipulation flag

        // Start a timer to determine if this is a manipulation
        _manipulationTokenSource = new CancellationTokenSource();
        Task.Delay(SelectionThresholdMilliseconds, _manipulationTokenSource.Token)
            .ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    _isManipulating = true; // Set flag if the delay completes
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        _isPressed = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs? e)
    {
        // Cancel the manipulation timer
        _manipulationTokenSource.Cancel();

        if (!_isManipulating)
        {
            if (!_isPressed) return;
            _isPressed = false;
            
            var position = e?.GetPosition(MapCanvas);
            if (!position.HasValue) return;
            var selectedHex = MapCanvas.Children
                .OfType<HexControl>()
                .FirstOrDefault(h => h.IsPointInside(position.Value));

            if (selectedHex != null && ViewModel!=null)
            {
                // Assign the hex coordinates to the ViewModel's unit position
                ViewModel?.HandleHexSelection(selectedHex.Hex);
            }
        }
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

        if (!(newScale >= MinScale) || !(newScale <= MaxScale)) return;
        _mapScaleTransform.ScaleX = newScale;
        _mapScaleTransform.ScaleY = newScale;
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
        if (ViewModel is { Game: not null })
        {
            RenderMap(ViewModel.Game, (IImageService<Bitmap>)ViewModel.ImageService);
        }
    }
}
