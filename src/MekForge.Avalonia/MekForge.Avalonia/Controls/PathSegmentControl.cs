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
        if (_segment.IsTurn)
        {
            var arcGeometry = new StreamGeometry();
            using var context = arcGeometry.Open();
            var startAngle = _segment.TurnAngleStart * Math.PI / 180;
            var sweepAngle = _segment.TurnAngleSweep * Math.PI / 180;
                
            context.BeginFigure(
                new Point(_segment.StartX, _segment.StartY), 
                false);
                
            context.ArcTo(
                new Point(_segment.EndX, _segment.EndY),
                new Size(HexCoordinates.HexWidth / 2, HexCoordinates.HexHeight / 2),
                0,
                false, // sweepAngle is always 60 or -60 degrees
                sweepAngle > 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise
            );
            return arcGeometry;
        }
        else
        {
            return new LineGeometry
            {
                StartPoint = new Point(_segment.StartX, _segment.StartY),
                EndPoint = new Point(_segment.EndX, _segment.EndY)
            };
        }
    }
}
