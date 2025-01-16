using Sanet.MekForge.Core.Models.Map;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class PathSegmentViewModel : BaseViewModel
{
    private readonly HexPosition _from;
    private readonly HexPosition _to;
    private const double InitialLineLength = HexCoordinates.HexHeight * 0.4;

    public PathSegmentViewModel(HexPosition from, HexPosition to)
    {
        _from = from;
        _to = to;
    }

    public HexPosition From => _from;
    public HexPosition To => _to;
    
    public bool IsTurn => _from.Coordinates == _to.Coordinates && _from.Facing != _to.Facing;

    // Screen coordinates for positioning the control
    public double FromX => _from.Coordinates.H;
    public double FromY => _from.Coordinates.V;

    // Relative coordinates for path drawing (center of control is at From position)
    public double StartX => HexCoordinates.HexWidth;
    public double StartY => HexCoordinates.HexHeight;

    // For turns - initial line segment
    public double InitialLineEndX => StartX + InitialLineLength * Math.Cos((int)_from.Facing * Math.PI / 3);
    public double InitialLineEndY => StartY + InitialLineLength * Math.Sin((int)_from.Facing * Math.PI / 3);
    
    // For turns - arc end point
    public double EndX => IsTurn 
        ? InitialLineEndX + (HexCoordinates.HexWidth / 3) * Math.Cos((int)_to.Facing * Math.PI / 3) 
        : StartX + (_to.Coordinates.H - _from.Coordinates.H);
        
    public double EndY => IsTurn 
        ? InitialLineEndY + (HexCoordinates.HexHeight / 3) * Math.Sin((int)_to.Facing * Math.PI / 3)
        : StartY + (_to.Coordinates.V - _from.Coordinates.V);

    // Direction vector at the end point (normalized)
    public (double X, double Y) EndDirectionVector
    {
        get
        {
            if (IsTurn)
            {
                // For turns, the direction is the final facing direction
                var angle = (int)_to.Facing * Math.PI / 3;
                return (Math.Cos(angle), Math.Sin(angle));
            }
            else
            {
                // For lines, normalize the direction vector
                var dx = EndX - StartX;
                var dy = EndY - StartY;
                var length = Math.Sqrt(dx * dx + dy * dy);
                return (dx / length, dy / length);
            }
        }
    }

    // For turns
    public double TurnAngleStart => ((int)_from.Facing * 60) % 360;
    public double TurnAngleSweep
    {
        get
        {
            if (!IsTurn) return 0;
            var fromAngle = (int)_from.Facing;
            var toAngle = (int)_to.Facing;
            
            // For single step turns, we only need to determine if it's clockwise or counterclockwise
            var clockwise = (toAngle - fromAngle + 6) % 6 == 1;
            return clockwise ? 30 : -30; // Reduced to 30 degrees
        }
    }
}
