using Sanet.MekForge.Core.Models.Units.Components.Internal;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class Head : UnitPart
{
    public Head(int maxArmor, int maxStructure) 
        : base("Head", PartLocation.Head, maxArmor, maxStructure, 12)
    {
        // Add default components
        Components.Add(new LifeSupport());
        Components.Add(new Sensors());
        Components.Add(new Cockpit());
    }
}