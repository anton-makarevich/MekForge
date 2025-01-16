using Sanet.MekForge.Core.Models.Map;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MekForge.Core.ViewModels;

public class PathSegmentViewModel : BaseViewModel
{
    private readonly HexPosition _from;
    private readonly HexPosition _to;

    public PathSegmentViewModel(HexPosition from, HexPosition to)
    {
        _from = from;
        _to = to;
    }

    public HexPosition From => _from;
    public HexPosition To => _to;
    
    public bool IsTurn => _from.Coordinates == _to.Coordinates && _from.Facing != _to.Facing;

    // Screen coordinates for rendering
    public double FromX => _from.Coordinates.H;
    public double FromY => _from.Coordinates.V;
    public double ToX => _to.Coordinates.H;
    public double ToY => _to.Coordinates.V;

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
            return clockwise ? 60 : -60;
        }
    }

    // For movement lines
    public double MovementAngle
    {
        get
        {
            if (IsTurn) return 0;
            var direction = _from.Coordinates.GetDirectionToNeighbour(_to.Coordinates);
            return ((int)direction * 60) % 360;
        }
    }

    public double Length
    {
        get
        {
            if (IsTurn) return HexCoordinates.HexWidth / 2; // Radius for turn arc
            return HexCoordinates.HexWidth * 0.75; // Distance between hex centers
        }
    }
}
