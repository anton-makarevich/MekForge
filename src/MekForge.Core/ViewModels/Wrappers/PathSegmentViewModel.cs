using System.Numerics;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels.Wrappers;

public class PathSegmentViewModel : BaseViewModel
{
    private readonly HexPosition _from;
    private readonly HexPosition _to;
    private const double TurnLength = 40;

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
    
    public double EndX => IsTurn 
        ? StartX + TurnLength * Math.Sin((int)_to.Facing * Math.PI / 3)
        : StartX + (_to.Coordinates.H - _from.Coordinates.H);
        
    public double EndY => IsTurn 
        ? StartY - TurnLength * Math.Cos((int)_to.Facing * Math.PI / 3)
        : StartY + (_to.Coordinates.V - _from.Coordinates.V);

    // Direction vector at the end point (normalized)
    public Vector2 ArrowDirectionVector
    {
        get
        {
           var angle = (int)_to.Facing * Math.PI / 3;
           return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }
    }

    public double TurnAngleSweep
    {
        get
        {
            if (!IsTurn) return 0;
            var fromAngle = (int)_from.Facing;
            var toAngle = (int)_to.Facing;
            
            // For single step turns, we only need to determine if it's clockwise or counterclockwise
            var clockwise = (toAngle - fromAngle + 6) % 6 == 1;
            return clockwise ? 60 : -60;
        }
    }
}
