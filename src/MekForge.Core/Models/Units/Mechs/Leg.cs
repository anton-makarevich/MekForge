namespace Sanet.MekForge.Core.Models.Units.Mechs;

using Components.Internal.Actuators;

public class Leg : UnitPart
{
    public Leg(PartLocation location, int maxArmor, int maxStructure) 
        : base($"{location} Leg", location, maxArmor, maxStructure, 12)
    {
        // Add default components
        Components.Add(new Hip());
        Components.Add(new UpperLegActuator());
        Components.Add(new LowerLegActuator());
        Components.Add(new FootActuator());
    }
}