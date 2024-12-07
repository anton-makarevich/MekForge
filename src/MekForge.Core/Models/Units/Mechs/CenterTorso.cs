using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class CenterTorso : Torso
{
    public CenterTorso(int maxArmor, int maxRearArmor, int maxStructure) 
        : base("Center Torso", PartLocation.CenterTorso, maxArmor, maxRearArmor, maxStructure)
    {
        // Add default components
        TryAddComponent(new Gyro());
    }
}