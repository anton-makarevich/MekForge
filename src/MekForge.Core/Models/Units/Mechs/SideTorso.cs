namespace Sanet.MekForge.Core.Models.Units.Mechs;

public class SideTorso : Torso
{
    public SideTorso(PartLocation location, int maxArmor, int maxRearArmor, int maxStructure) 
        : base($"{location} Torso", location,maxArmor, maxRearArmor, maxStructure)
    {

    }
}