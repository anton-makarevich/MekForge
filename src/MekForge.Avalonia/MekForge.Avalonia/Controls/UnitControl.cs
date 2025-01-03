using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using System.Reactive.Linq;
using System.Threading;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Avalonia.Controls
{
    public class UnitControl : Grid, IDisposable
    {
        private readonly Image _unitImage;
        private readonly IImageService<Bitmap> _imageService;
        private readonly Unit _unit;
        private readonly IDisposable _subscription;
        private readonly Border _tintBorder;

        public UnitControl(Unit unit, IImageService<Bitmap> imageService)
        {
            _unit = unit;
            _imageService = imageService;
            
            Width = HexCoordinates.HexWidth;
            Height = HexCoordinates.HexHeight;
            
            _unitImage = new Image
            {
                Width = Width * 0.84,
                Height = Height * 0.84,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _tintBorder = new Border
            {
                Width = _unitImage.Width,
                Height = _unitImage.Height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Colors.White), // Will be updated with player color
                Opacity = 0.7
            };
            
            Children.Add(_unitImage);
            Children.Add(_tintBorder);

            // Create an observable that polls the unit's position
             Observable
                .Interval(TimeSpan.FromMilliseconds(32)) // ~60fps
                .Select(_ => new
                {
                    _unit.Position,
                    _unit.IsDeployed    ,
                    _unit.Facing
                })
                .DistinctUntilChanged()
                .ObserveOn(SynchronizationContext.Current) // Ensure events are processed on the UI thread
                .Subscribe(_ => Render());
            
            // Initial update
            Render();
            UpdateImage();
        }

        private void Render()
        {
            IsVisible = _unit.IsDeployed;
            if (_unit.Position == null) return;
            var hexPosition = _unit.Position;
            SetValue(Canvas.LeftProperty, hexPosition.Value.H);
            SetValue(Canvas.TopProperty, _unit.Position.Value.V);
            double rotationAngle = _unit.Facing switch
            {
                HexDirection.Top => 0,
                HexDirection.TopRight => 60,
                HexDirection.BottomRight => 120,
                HexDirection.Bottom => 180,
                HexDirection.BottomLeft => 240,
                HexDirection.TopLeft => 300,
                _ => 0
            };

            RenderTransform = new RotateTransform(rotationAngle, 0, 0);
        }
        
        private void UpdateImage()
        {
            var image = _imageService.GetImage("units/mechs", _unit.Model.ToUpper());
            if (image != null)
            {
                _unitImage.Source = image;
            }
            
            // Apply player's tint color if available
            if (_unit.Owner == null) return;
            var color = Color.Parse(_unit.Owner.Tint);
            _tintBorder.OpacityMask = new ImageBrush { Source = image, Stretch = Stretch.Fill };
            _tintBorder.Background = new SolidColorBrush(color);
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}
