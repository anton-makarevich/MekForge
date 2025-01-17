using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.ViewModels;

namespace Sanet.MekForge.Avalonia.Controls;

public class PathSegmentControl : Panel
{
    private readonly PathSegmentViewModel _segment;
    private static readonly IBrush PathStroke = Brushes.Yellow;
    private const double StrokeThickness = 2;
    private const double ArrowSize = 10; // Size of arrow head

    public PathSegmentControl(PathSegmentViewModel segment)
    {
        _segment = segment;
        
        Width = HexCoordinates.HexWidth * 2;
        Height = HexCoordinates.HexHeight * 2;
        
        var path = new Path
        {
            Stroke = PathStroke,
            StrokeThickness = StrokeThickness,
            Data = CreatePathGeometry()
        };
        
        Children.Add(path);
        
        // Position control so that From coordinates are at its center
        SetValue(Canvas.LeftProperty, _segment.FromX - HexCoordinates.HexWidth*0.5);
        SetValue(Canvas.TopProperty, _segment.FromY - HexCoordinates.HexHeight*0.5);
    }

    private Geometry CreatePathGeometry()
    {
        var geometry = new GeometryGroup();
        
        if (_segment.IsTurn)
        {
            // Arc from line end to final position
            var arcGeometry = new StreamGeometry();
            using (var context = arcGeometry.Open())
            {
                var sweepAngle = _segment.TurnAngleSweep * Math.PI / 180;
                
                context.BeginFigure(
                    new Point(_segment.StartX, _segment.StartY), 
                    false);
                
                context.ArcTo(
                    new Point(_segment.EndX, _segment.EndY),
                    new Size(HexCoordinates.HexWidth / 2, HexCoordinates.HexHeight / 2),
                    0,
                    false,
                    sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise
                );
            }
            geometry.Children.Add(arcGeometry);
        }
        else
        {
            geometry.Children.Add(new LineGeometry
            {
                StartPoint = new Point(_segment.StartX, _segment.StartY),
                EndPoint = new Point(_segment.EndX, _segment.EndY)
            });
        }

        // Add arrow at the end
        var (dirX, dirY) = _segment.EndDirectionVector;
        var endPoint = new Point(_segment.EndX, _segment.EndY);
        var arrowGeometry = new StreamGeometry();
        using (var context = arrowGeometry.Open())
        {
            // Calculate arrow points
            var leftPoint = new Point(
                endPoint.X - ArrowSize * (dirX * 0.866 - dirY * 0.5), // cos(30°) = 0.866
                endPoint.Y - ArrowSize * (dirY * 0.866 + dirX * 0.5)  // sin(30°) = 0.5
            );
            var rightPoint = new Point(
                endPoint.X - ArrowSize * (dirX * 0.866 + dirY * 0.5),
                endPoint.Y - ArrowSize * (dirY * 0.866 - dirX * 0.5)
            );

            context.BeginFigure(endPoint, true);
            context.LineTo(leftPoint);
            context.LineTo(rightPoint);
            context.LineTo(endPoint);
        }
        geometry.Children.Add(arrowGeometry);

        return geometry;
    }
}
