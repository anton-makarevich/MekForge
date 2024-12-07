using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class CenterTorso : UnitPart
{
    public CenterTorso(int maxArmor, int maxRearArmor, int maxStructure) 
        : base("Center Torso", PartLocation.CenterTorso, maxArmor, maxRearArmor, maxStructure)
    {
        // Add default components
        Components.Add(new Gyro());
    }
}