using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Utils.MechData;

public class MechFactory
{
    private readonly IStructureValueProvider _structureValueProvider;

    public MechFactory( IStructureValueProvider structureValueProvider)
    {
        _structureValueProvider = structureValueProvider;
    }

    public Mech Create(MechData mechData)
    {
        
        // Create parts with appropriate armor and structure
        var parts = CreateParts(mechData.ArmorValues, _structureValueProvider, mechData.Mass);
        
        // Create the mech
        var mech = new Mech(
            mechData.Chassis,
            mechData.Model,
            mechData.Mass,
            mechData.WalkMp,
            parts);

        // Add equipment to parts
        AddEquipmentToParts(mech, mechData.LocationEquipment);

        return mech;
    }

    private static List<UnitPart> CreateParts(Dictionary<PartLocation, ArmorLocation> armorValues, IStructureValueProvider structureValueProvider, int tonnage)
    {
        var structureValues = structureValueProvider.GetStructureValues(tonnage);
        var parts = new List<UnitPart>();
        foreach (var (location, armor) in armorValues)
        {
            UnitPart part = location switch
            {
                PartLocation.LeftArm or PartLocation.RightArm => new Arm(location, armor.FrontArmor, structureValues[location]),
                PartLocation.LeftTorso or PartLocation.RightTorso => new SideTorso(location, armor.FrontArmor, armor.RearArmor, structureValues[location]),
                PartLocation.CenterTorso => new CenterTorso(armor.FrontArmor, armor.RearArmor, structureValues[location]),
                PartLocation.Head => new Head(armor.FrontArmor, structureValues[location]),
                PartLocation.LeftLeg or PartLocation.RightLeg => new Leg(location, armor.FrontArmor, structureValues[location]),
                _ => throw new ArgumentException($"Unknown location: {location}")
            };
            parts.Add(part);
        }
        return parts;
    }

    private static void AddEquipmentToParts(Mech mech, Dictionary<PartLocation, List<string>> locationEquipment)
    {
        foreach (var (location, equipment) in locationEquipment)
        {
            var part = mech.Parts.First(p => p.Location == location);
            foreach (var item in equipment)
            {
                var component = CreateComponent(item);
                if (component != null)
                    part.TryAddComponent(component);
            }
        }
    }

    private static Component? CreateComponent(string itemName) => itemName switch
    {
        "Machine Gun" => new MachineGun(),
        "Medium Laser" => new MediumLaser(),
        "Heat Sink" => new HeatSink(),
        "Shoulder" => new Shoulder(),
        "Upper Arm Actuator" => new UpperArmActuator(),
        "Fusion Engine" => new Engine("Fusion Engine", 160),
        _ => null
    };
}
