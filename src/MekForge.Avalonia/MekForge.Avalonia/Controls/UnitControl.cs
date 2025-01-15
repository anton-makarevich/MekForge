using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Services;
using System.Reactive.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Interactivity;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.UiStates;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.Controls
{
    public class UnitControl : Grid, IDisposable
    {
        private readonly Image _unitImage;
        private readonly IImageService<Bitmap> _imageService;
        private readonly Unit _unit;
        private readonly IDisposable _subscription;
        private readonly Border _tintBorder;
        private readonly BattleMapViewModel _viewModel;

        public UnitControl(Unit unit, IImageService<Bitmap> imageService, BattleMapViewModel viewModel)
        {
            _unit = unit;
            _imageService = imageService;
            _viewModel = viewModel;

            IsHitTestVisible = false;
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
                Background = new SolidColorBrush(Colors.White),
                Opacity = 0.7
            };
            
            MovementButtons = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsVisible = false,
                Spacing = 4,
                Margin = new Thickness(4)
            };

            var walkButton = CreateMovementButton("Walk", MovementType.Walk);
            var runButton = CreateMovementButton("Run", MovementType.Run);
            var jumpButton = CreateMovementButton("Jump", MovementType.Jump);
            jumpButton.IsVisible = _unit.GetMovementPoints(MovementType.Jump)>0;

            MovementButtons.Children.Add(walkButton);
            MovementButtons.Children.Add(runButton);
            MovementButtons.Children.Add(jumpButton);

            var color = _unit.Owner!=null 
                ?Color.Parse(_unit.Owner.Tint)
                :Colors.Yellow;
            var selectionBorder = new Border
            {
                Width = Width*0.9,
                Height = Height*0.9,
                Opacity = 0.9,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(4),
                CornerRadius = new CornerRadius(Width/2),
                IsVisible = false
            };

            Children.Add(selectionBorder);
            Children.Add(_unitImage);
            Children.Add(_tintBorder);

            // Create an observable that polls the unit's position and selection state
             Observable
                .Interval(TimeSpan.FromMilliseconds(32)) // ~60fps
                .Select(_ => new
                {
                    _unit.Position,
                    _unit.IsDeployed,
                    _viewModel.SelectedUnit
                })
                .DistinctUntilChanged()
                .ObserveOn(SynchronizationContext.Current) // Ensure events are processed on the UI thread
                .Subscribe(state => 
                {
                    Render();
                    selectionBorder.IsVisible = state.SelectedUnit == _unit;
                    MovementButtons.IsVisible = state.SelectedUnit == _unit
                                                && _viewModel.CurrentState is MovementState
                                                    { CurrentMovementStep: MovementStep.SelectingMovementType };
                });
            
            // Initial update
            Render();
            UpdateImage();
        }

        private void Render()
        {
            IsVisible = _unit.IsDeployed;
            if (_unit.Position == null) return;
            var hexPosition = _unit.Position;
            
            var leftPos = hexPosition.Value.Coordinates.H;
            var topPos = hexPosition.Value.Coordinates.V;
            
            SetValue(Canvas.LeftProperty, leftPos);
            SetValue(Canvas.TopProperty, topPos);
            
            // Update buttons position to follow the unit
            if (MovementButtons.Parent == null && Parent is Canvas canvas)
            {
                canvas.Children.Add(MovementButtons);
            }
            Canvas.SetLeft(MovementButtons, leftPos);
            Canvas.SetTop(MovementButtons, topPos + Height);
            
            double rotationAngle = _unit.Position.Value.Facing switch
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

        private Button CreateMovementButton(string text, MovementType type)
        {
            var button = new Button
            {
                Background = new SolidColorBrush(Colors.Aqua),
                Padding = new Thickness(8, 4),
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var content = new TextBlock
            {
                Text = $"{text} | MP: {_unit.GetMovementPoints(type)}",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            button.Content = content;

            button.Click += (_, _) =>
            {
                if (_viewModel.CurrentState is not MovementState state) return;
                state.HandleMovementTypeSelection(type);
                MovementButtons.IsVisible = false;
            };

            return button;
        }

        public bool HandleInteraction(Point position)
        {
            if (!MovementButtons.IsVisible)
                return false;
            
            // Find which button was clicked based on position
            var clickedButton = MovementButtons.Children
                .OfType<Button>()
                .FirstOrDefault(b => b.Bounds.Contains(position));

            if (clickedButton is not {IsEnabled:true, IsVisible:true}) return false;
            // Trigger the button's Click event
            clickedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            return true;
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
        
        public StackPanel MovementButtons { get; }
    }
}
