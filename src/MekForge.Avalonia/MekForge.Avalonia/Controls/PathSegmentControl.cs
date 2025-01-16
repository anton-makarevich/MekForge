using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.Controls;

public class PathSegmentControl : Panel
{
    private readonly Path _path;
    private readonly PathSegmentViewModel _segment;
    private static readonly IBrush PathStroke = Brushes.Yellow;
    private const double StrokeThickness = 2;

    public PathSegmentControl(PathSegmentViewModel segment)
    {
        _segment = segment;
        _path = new Path
        {
            Stroke = PathStroke,
            StrokeThickness = StrokeThickness,
            Data = CreatePathGeometry()
        };
        
        Children.Add(_path);
        UpdatePathSegmentPosition();
    }

    private Geometry CreatePathGeometry()
    {
        if (_segment.IsTurn)
        {
            var center = new Point(_segment.Length, _segment.Length);
            var radius = _segment.Length;
            
            var arcGeometry = new StreamGeometry();
            using var context = arcGeometry.Open();
            var startAngle = _segment.TurnAngleStart * Math.PI / 180;
            var sweepAngle = _segment.TurnAngleSweep * Math.PI / 180;
                
            var startPoint = new Point(
                center.X + radius * Math.Cos(startAngle),
                center.Y + radius * Math.Sin(startAngle)
            );

            context.BeginFigure(startPoint, false);
            context.ArcTo(
                new Point(
                    center.X + radius * Math.Cos(startAngle + sweepAngle),
                    center.Y + radius * Math.Sin(startAngle + sweepAngle)
                ),
                new Size(radius, radius),
                0,
                false, // sweepAngle is always 60 or -60 degrees, so never more than 180
                sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise
            );
            return arcGeometry;
        }
        else
        {
            var lineGeometry = new LineGeometry
            {
                StartPoint = new Point(0, 2),
                EndPoint = new Point(_segment.Length, 2)
            };
            return lineGeometry;
        }
    }
    
    private void UpdatePathSegmentPosition()
    {
        if (_segment.IsTurn)
        {
            // For turns, place the control centered on the hex
            SetValue(Canvas.LeftProperty, _segment.FromX - _segment.Length);
            SetValue(Canvas.TopProperty, _segment.FromY - _segment.Length);
            Width = _segment.Length * 2;
            Height = _segment.Length * 2;
        }
        else
        {
            // For movements, place and rotate the line between hexes
            SetValue(Canvas.LeftProperty, _segment.FromX);
            SetValue(Canvas.TopProperty, _segment.FromY - 2);
            Width = _segment.Length;
            Height = 4;
            
            var rotateTransform = new RotateTransform(_segment.MovementAngle)
            {
                CenterX = 0,
                CenterY = 2
            };
            RenderTransform = rotateTransform;
        }
    }
}
