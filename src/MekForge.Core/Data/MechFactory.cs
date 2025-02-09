using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Models.Units.Components;
using Sanet.MekForge.Core.Models.Units.Components.Engines;
using Sanet.MekForge.Core.Models.Units.Components.Internal.Actuators;
using Sanet.MekForge.Core.Models.Units.Components.Weapons;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Ballistic;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Energy;
using Sanet.MekForge.Core.Models.Units.Components.Weapons.Missile;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Data;

public class MechFactory
{
    private readonly IRulesProvider _rulesProvider;

    public MechFactory( IRulesProvider rulesProvider)
    {
        _rulesProvider = rulesProvider;
    }

    public Mech Create(Data.UnitData unitData)
    {
        
        // Create parts with appropriate armor and structure
        var parts = CreateParts(unitData.ArmorValues, _rulesProvider, unitData.Mass);
        
        // Create the mech
        var mech = new Mech(
            unitData.Chassis,
            unitData.Model,
            unitData.Mass,
            unitData.WalkMp,
            parts,
            1,
            unitData.Id);
        
        // Add equipment to parts
        AddEquipmentToParts(mech, unitData);

        return mech;
    }

    private List<UnitPart> CreateParts(Dictionary<PartLocation, ArmorLocation> armorValues, IRulesProvider rulesProvider, int tonnage)
    {
        var structureValues = rulesProvider.GetStructureValues(tonnage);
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

    private void AddEquipmentToParts(Mech mech, Data.UnitData unitData)
    {
        foreach (var (location, equipment) in unitData.LocationEquipment)
        {
            var part = mech.Parts.First(p => p.Location == location);
            var componentCounts = new Dictionary<MekForgeComponent, int>(); // Track component counts

            foreach (var item in equipment)
            {
                componentCounts.TryAdd(item, 0);
                componentCounts[item]++;

                var component = CreateComponent(item, unitData);
                if (component == null || (componentCounts[item] < component.Size && component is not Engine)) continue;
                part.TryAddComponent(component);
                componentCounts[item] = 0; // Reset count after adding
            }
        }
    }

    private Component? CreateComponent(MekForgeComponent itemName, Data.UnitData unitData)
    {
        return itemName switch
        {
            MekForgeComponent.Engine => new Engine(unitData.EngineRating, MapEngineType(unitData.EngineType)),
            MekForgeComponent.ISAmmoAC5 => new Ammo(AmmoType.AC5, _rulesProvider.GetAmmoRounds(AmmoType.AC5)),
            MekForgeComponent.ISAmmoSRM2 => new Ammo(AmmoType.SRM2, _rulesProvider.GetAmmoRounds(AmmoType.SRM2)),
            MekForgeComponent.ISAmmoMG => new Ammo(AmmoType.MachineGun, _rulesProvider.GetAmmoRounds(AmmoType.MachineGun)),
            MekForgeComponent.ISAmmoLRM5 => new Ammo(AmmoType.LRM5, _rulesProvider.GetAmmoRounds(AmmoType.LRM5)),
            MekForgeComponent.MediumLaser => new MediumLaser(),
            MekForgeComponent.LRM5 => new LRM5(),
            MekForgeComponent.SRM2 => new SRM2(),
            MekForgeComponent.MachineGun => new MachineGun(),
            MekForgeComponent.AC5 => new AC5(),
            MekForgeComponent.HeatSink => new HeatSink(),
            MekForgeComponent.Shoulder => new Shoulder(),
            MekForgeComponent.UpperArmActuator => new UpperArmActuator(),
            MekForgeComponent.LowerArmActuator => new LowerArmActuator(),
            MekForgeComponent.HandActuator => new HandActuator(),
            MekForgeComponent.JumpJet => new JumpJets(),
            MekForgeComponent.Gyro => null,
            MekForgeComponent.LifeSupport => null,
            MekForgeComponent.Sensors => null,
            MekForgeComponent.Cockpit => null,
            MekForgeComponent.Hip => null,
            MekForgeComponent.UpperLegActuator => null,
            MekForgeComponent.LowerLegActuator => null,
            MekForgeComponent.FootActuator => null,
            _ => throw new NotImplementedException($"{itemName} is not implemented")
        };
    }

    private EngineType MapEngineType(string engineType)
    {
        return engineType.ToLower() switch
        {
            "fusion" => EngineType.Fusion,
            "xlfusion" => EngineType.XLFusion,
            "ice" => EngineType.ICE,
            "light" => EngineType.Light,
            "compact" => EngineType.Compact,
            _ => throw new NotImplementedException($"Unknown engine type: {engineType}")
        };
    }
}
