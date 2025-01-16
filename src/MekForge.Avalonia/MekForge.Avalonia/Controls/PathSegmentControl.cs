using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.Controls;

public class PathSegmentControl : Control
{
    private readonly PathSegmentViewModel _segment;

    public PathSegmentControl(PathSegmentViewModel segment)
    {
        _segment = segment;
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        var pen = new Pen(Brushes.Yellow, 2);

        if (_segment.IsTurn)
        {
            // Draw an arc for turning
            var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            var radius = _segment.Length;
            
            var arcGeometry = new StreamGeometry();
            using (var context1 = arcGeometry.Open())
            {
                var startAngle = _segment.TurnAngleStart * Math.PI / 180;
                var sweepAngle = _segment.TurnAngleSweep * Math.PI / 180;
                
                var startPoint = new Point(
                    center.X + radius * Math.Cos(startAngle),
                    center.Y + radius * Math.Sin(startAngle)
                );

                context1.BeginFigure(startPoint, false);
                context1.ArcTo(
                    new Point(
                        center.X + radius * Math.Cos(startAngle + sweepAngle),
                        center.Y + radius * Math.Sin(startAngle + sweepAngle)
                    ),
                    new Size(radius, radius),
                    0,
                    sweepAngle > Math.PI,
                    sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise
                );
            }

            context.DrawGeometry(null, pen, arcGeometry);
        }
        else
        {
            // Draw a line for movement
            var start = new Point(0, Bounds.Height / 2);
            var end = new Point(_segment.Length, Bounds.Height / 2);
            context.DrawLine(pen, start, end);
        }
        UpdatePathSegmentPosition();
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
            SetValue(Canvas.LeftProperty, _segment.FromX - _segment.Length);
            SetValue(Canvas.TopProperty, _segment.FromY - _segment.Length);
            Width = _segment.Length;
            Height = 4;
            
            var rotateTransform = new RotateTransform(_segment.MovementAngle);
            rotateTransform.CenterX = 0;
            rotateTransform.CenterY = 2;
            RenderTransform = rotateTransform;
        }
    }
}
