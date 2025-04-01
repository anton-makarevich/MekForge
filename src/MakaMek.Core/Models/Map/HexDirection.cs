namespace Sanet.MakaMek.Core.Models.Map;

public enum HexDirection
{
    Top = 0,         
    TopRight = 1,    
    BottomRight = 2, 
    Bottom = 3,      
    BottomLeft = 4, 
    TopLeft = 5 
}

public static class HexDirectionExtensions
{
    public static HexDirection GetOppositeDirection(this HexDirection direction) =>
        (HexDirection)((int)(direction + 3) % 6);
}
