using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MekForge.Core.Models;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;

namespace Sanet.MekForge.Avalonia.Controls
{
    public class UnitControl : Grid
    {
        private readonly Image _unitImage;
        private readonly IImageService<Bitmap> _imageService;
        private Unit _unit;

        public UnitControl(Unit unit, IImageService<Bitmap> imageService)
        {
            _unit = unit;
            _imageService = imageService;
            
            Width = HexCoordinates.HexWidth;
            Height = HexCoordinates.HexHeight;
            
            _unitImage = new Image
            {
                Width = Width*0.84,
                Height = Height*.84,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Children.Add(_unitImage);
        }
        
        public void Update()
        {
            UpdatePosition();
            UpdateImage();
        }

        private void UpdatePosition()
        {
            if (_unit?.Position == null) return;
            var hexPosition = _unit.Position;
            SetValue(Canvas.LeftProperty, hexPosition.Value.X);
            SetValue(Canvas.TopProperty, _unit.Position.Value.Y);            
        }
        
        private void UpdateImage()
        {
            var image = _imageService.GetImage("units/mechs", _unit.Model.ToUpper());
            if (image != null)
            {
                _unitImage.Source = image;
            }
        }
    }
}
