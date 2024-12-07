using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Sanet.MekForge.Core.Models.Units.Components.Internal;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Sanet.MekForge.Core.Utils;

namespace Sanet.MekForge.Core.Models.Units.Mechs;

public static class MechFactory
{
    public static async Task<Mech> CreateFromMtfFileAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return CreateFromMtfData(lines);
    }

    public static Mech CreateFromMtfData(IEnumerable<string> mtfData)
    {
        var parser = new MtfParser();
        var mechData = parser.Parse(mtfData);
        
        // Create parts with appropriate armor and structure
        var parts = CreateParts(mechData.ArmorValues);
        
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

    private static List<UnitPart> CreateParts(Dictionary<PartLocation, ArmorValues> armorValues)
    {
        var parts = new List<UnitPart>();
        foreach (var (location, armor) in armorValues)
        {
            UnitPart part = location switch
            {
                PartLocation.LeftArm or PartLocation.RightArm => new Arm(location, armor.FrontArmor, 6),
                PartLocation.LeftTorso or PartLocation.RightTorso => new SideTorso(location, armor.FrontArmor, armor.RearArmor, 8),
                PartLocation.CenterTorso => new CenterTorso(armor.FrontArmor, armor.RearArmor, 10),
                PartLocation.Head => new Head(armor.FrontArmor, 3),
                PartLocation.LeftLeg or PartLocation.RightLeg => new Leg(location, armor.FrontArmor, 4),
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
