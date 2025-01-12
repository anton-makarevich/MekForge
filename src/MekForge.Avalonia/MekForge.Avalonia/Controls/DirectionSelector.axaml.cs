using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Sanet.MekForge.Core.Models.Map;

namespace Sanet.MekForge.Avalonia.Controls
{
    public partial class DirectionSelector : UserControl
    {
        public DirectionSelector()
        {
            InitializeComponent();
        }
        
        public static readonly StyledProperty<ICommand?> DirectionSelectedCommandProperty =
            AvaloniaProperty.Register<DirectionSelector, ICommand?>(
                nameof(DirectionSelectedCommand));

        public ICommand? DirectionSelectedCommand
        {
            get => GetValue(DirectionSelectedCommandProperty);
            set => SetValue(DirectionSelectedCommandProperty, value);
        }

        public static readonly DirectProperty<DirectionSelector,IEnumerable<HexDirection>?> EnabledDirectionsProperty =
            AvaloniaProperty.RegisterDirect<DirectionSelector, IEnumerable<HexDirection>?>(
                nameof(EnabledDirections), o => o.EnabledDirections,
                (o, v) => o.EnabledDirections = v);

        private IEnumerable<HexDirection>? _enabledDirections;
        public IEnumerable<HexDirection>? EnabledDirections
        {
            get => _enabledDirections;
            set
            {
                SetAndRaise(EnabledDirectionsProperty, ref _enabledDirections, value);
                TopButton.IsEnabled = value?.Contains(HexDirection.Top) ?? false;
                TopRightButton.IsEnabled = value?.Contains(HexDirection.TopRight) ?? false;
                BottomRightButton.IsEnabled = value?.Contains(HexDirection.BottomRight) ?? false;
                BottomButton.IsEnabled = value?.Contains(HexDirection.Bottom) ?? false;
                BottomLeftButton.IsEnabled = value?.Contains(HexDirection.BottomLeft) ?? false;
                TopLeftButton.IsEnabled = value?.Contains(HexDirection.TopLeft) ?? false;
            }
        }

        public static readonly DirectProperty<DirectionSelector,Point> PositionProperty =
            AvaloniaProperty.RegisterDirect<DirectionSelector, Point>(
                nameof(Position),
                o => o.Position,
                (o, v) => o.Position = v);

        private Point _position;
        public Point Position
        {
            get => _position;
            set => SetAndRaise(PositionProperty, ref _position, value);
        }

        private void TopButton_Click(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.Top);
        }

        private void TopRightButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.TopRight);
        }

        private void BottomRightButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.BottomRight);
        }

        private void BottomButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.Bottom);
        }

        private void BottomLeftButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.BottomLeft);
        }

        private void TopLeftButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DirectionSelectedCommand?.Execute(HexDirection.TopLeft);
        }
    }
}
