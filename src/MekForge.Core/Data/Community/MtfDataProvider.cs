using System.Text.RegularExpressions;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Data.Community;

public class MtfDataProvider:IMechDataProvider
{
    private readonly Dictionary<string, string> _mechData = new();
    private readonly Dictionary<PartLocation, List<MekForgeComponent>> _locationEquipment = new();
    private readonly Dictionary<PartLocation, ArmorLocation> _armorValues = new();

    public Data.UnitData LoadMechFromTextData(IEnumerable<string> lines)
    {
        var listLines = lines.ToList();
        ParseBasicData(listLines);
        ParseLocationData(listLines);
        
        return new Data.UnitData
        {
            Chassis = _mechData["chassis"],
            Model = _mechData["model"],
            Mass = int.Parse(_mechData["Mass"]),
            WalkMp = int.Parse(Regex.Match(_mechData["Walk MP"], @"\d+").Value),
            EngineRating = int.Parse(_mechData["EngineRating"]),
            EngineType = _mechData["EngineType"],
            ArmorValues = _armorValues,
            LocationEquipment = _locationEquipment,
            Quirks = _mechData.Where(pair => pair.Key.StartsWith("quirk")).ToDictionary(),
            AdditionalAttributes = _mechData.Where(pair => pair.Key.StartsWith("system")).ToDictionary()
        };
    }

    private void ParseBasicData(IEnumerable<string> lines)
    {
        var quirksCount = 0;
        var systemsCount = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Config:"))
                continue;

            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;
            var key = line[..colonIndex].Trim();
            var value = line[(colonIndex + 1)..].Trim();

            if (key == "Engine")
            {
                var engineData = value.Split(' ');
                if (engineData.Length >= 2)
                {
                    _mechData["EngineRating"] = engineData[0];
                    _mechData["EngineType"] = engineData[1];
                }
            }
            else
            {
                if (key.StartsWith("quirk"))
                {
                    key = $"{key}{++quirksCount}";
                }
                if (key.StartsWith("system"))
                {
                    key = $"{key}{++systemsCount}";
                }
                _mechData[key] = value;
            }
        }
    }

    private void ParseLocationData(IEnumerable<string> lines)
    {
        PartLocation? currentLocation = null;
        var parsingArmor = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentLocation == PartLocation.RightLeg)
                {
                    return;
                }
                continue;
            }

            // Start of armor section
            if (line.StartsWith("Armor:"))
            {
                parsingArmor = true;
                continue;
            }

            // End of armor section
            if (line.StartsWith("Weapons:"))
            {
                parsingArmor = false;
                continue;
            }

            // Parse armor values
            if (parsingArmor)
            {
                var match = Regex.Match(line, @"(\w+)\s+Armor:(\d+)");
                if (match.Success && TryParseLocation(match.Groups[1].Value, out var location))
                {
                    var value = int.Parse(match.Groups[2].Value);
                    if (!_armorValues.ContainsKey(location))
                        _armorValues[location] = new ArmorLocation();

                    // Handle rear armor values
                    if (IsRearArmor(match.Groups[1].Value))
                    {
                        var mainLocation = GetMainLocationForRear(match.Groups[1].Value);
                        if (!_armorValues.ContainsKey(mainLocation))
                            _armorValues[mainLocation] = new ArmorLocation();
                        _armorValues[mainLocation].RearArmor = value;
                    }
                    else
                    {
                        _armorValues[location].FrontArmor = value;
                    }
                }
                continue;
            }

            // Check for location headers
            if (line.EndsWith(":"))
            {
                var locationText = line[..^1].Trim();
                if (TryParseLocation(locationText, out var location))
                {
                    currentLocation = location;
                    if (!_locationEquipment.ContainsKey(location))
                        _locationEquipment[location] = new List<MekForgeComponent>();
                }
                continue;
            }

            // Add equipment to current location
            if (!currentLocation.HasValue || line.Contains("-Empty-")) continue;
            {
                _locationEquipment[currentLocation.Value].Add(MapMtfStringToComponent(line));
            }
        }
    }

    private MekForgeComponent MapMtfStringToComponent(string mtfString)
    {
        return mtfString switch
        {
            "IS Ammo AC/5" => MekForgeComponent.ISAmmoAC5,
            "IS Ammo SRM-2" => MekForgeComponent.ISAmmoSRM2,
            "IS Ammo MG - Full" => MekForgeComponent.ISAmmoMG,
            "IS Ammo LRM-5" => MekForgeComponent.ISAmmoLRM5,
            "Medium Laser" => MekForgeComponent.MediumLaser,
            "LRM 5" => MekForgeComponent.LRM5,
            "SRM 2" => MekForgeComponent.SRM2,
            "Machine Gun" => MekForgeComponent.MachineGun,
            "Autocannon/5" => MekForgeComponent.AC5,
            "Heat Sink" => MekForgeComponent.HeatSink,
            "Shoulder" => MekForgeComponent.Shoulder,
            "Upper Arm Actuator" => MekForgeComponent.UpperArmActuator,
            "Lower Arm Actuator" => MekForgeComponent.LowerArmActuator,
            "Hand Actuator" => MekForgeComponent.HandActuator,
            "Jump Jet" => MekForgeComponent.JumpJet,
            "Fusion Engine" => MekForgeComponent.Engine,
            "Gyro" => MekForgeComponent.Gyro,
            "Life Support" => MekForgeComponent.LifeSupport,
            "Sensors" => MekForgeComponent.Sensors,
            "Cockpit" => MekForgeComponent.Cockpit,
            "Hip" => MekForgeComponent.Hip,
            "Upper Leg Actuator" => MekForgeComponent.UpperLegActuator,
            "Lower Leg Actuator" => MekForgeComponent.LowerLegActuator,
            "Foot Actuator" => MekForgeComponent.FootActuator,
            _ => throw new NotImplementedException($"Unknown MTF component: {mtfString}")
        };
    }

    private static bool TryParseLocation(string locationText, out PartLocation location)
    {
        location = locationText switch
        {
            "Left Arm" or "LA" => PartLocation.LeftArm,
            "Right Arm" or "RA" => PartLocation.RightArm,
            "Left Torso" or "LT" => PartLocation.LeftTorso,
            "Right Torso" or "RT" => PartLocation.RightTorso,
            "Center Torso" or "CT" => PartLocation.CenterTorso,
            "Head" or "HD" => PartLocation.Head,
            "Left Leg" or "LL" => PartLocation.LeftLeg,
            "Right Leg" or "RL" => PartLocation.RightLeg,
            "RTL" or "RTR" or "RTC" => GetMainLocationForRear(locationText),
            _ => throw new ArgumentException($"Unknown location: {locationText}")
        };
        return true;
    }

    private static bool IsRearArmor(string locationText)
    {
        return locationText is "RTL" or "RTR" or "RTC";
    }

    private static PartLocation GetMainLocationForRear(string rearLocationText) => rearLocationText switch
    {
        "RTL" => PartLocation.LeftTorso,
        "RTR" => PartLocation.RightTorso,
        "RTC" => PartLocation.CenterTorso,
        _ => throw new ArgumentException($"Invalid rear location: {rearLocationText}")
    };
}