using Sanet.MakaMek.Core.Models.Units.Components.Internal.Actuators;

namespace Sanet.MakaMek.Core.Models.Units.Mechs;

public class Arm : UnitPart
{
    public Arm(PartLocation location, int maxArmor, int maxStructure) 
        : base($"{location} Arm", location, maxArmor, maxStructure, 12)
    {
        // Add default components
        TryAddComponent(new Shoulder());
    }
}