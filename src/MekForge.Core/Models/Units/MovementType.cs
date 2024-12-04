namespace Sanet.MekForge.Core.Models.Units;

public enum MovementType
{
    Walk,   // Base movement
    Run,    // 1.5x walking
    Sprint, // 2x walking
    Jump,   // Requires jump jets
    Masc    // Requires MASC system
}
