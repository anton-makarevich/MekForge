using System.Text.RegularExpressions;
using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Utils.MechData.Community;

public class MtfDataProvider:IMechDataProvider
{
    private readonly Dictionary<string, string> _mechData = new();
    private readonly Dictionary<string, string> _additionalAttributes = new();
    private readonly Dictionary<PartLocation, List<string>> _locationEquipment = new();
    private readonly Dictionary<PartLocation, ArmorLocation> _armorValues = new();

    public MechData LoadMechFromTextData(IEnumerable<string> lines)
    {
        var listLines = lines.ToList();
        ParseBasicData(listLines);
        ParseLocationData(listLines);
        
        return new MechData
        {
            Chassis = _mechData["chassis"],
            Model = _mechData["model"],
            Mass = int.Parse(_mechData["Mass"]),
            WalkMp = int.Parse(Regex.Match(_mechData["Walk MP"], @"\d+").Value),
            ArmorValues = _armorValues,
            LocationEquipment = _locationEquipment,
            AdditionalAttributes = _additionalAttributes
        };
    }

    private void ParseBasicData(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Config:") || line.StartsWith("quirk:"))
                continue;

            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;
            var key = line[..colonIndex].Trim();
            var value = line[(colonIndex + 1)..].Trim();
            _mechData[key] = value;
        }
    }

    private void ParseLocationData(IEnumerable<string> lines)
    {
        PartLocation? currentLocation = null;
        var parsingArmor = false;
        var parsingAttributes = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentLocation == PartLocation.RightLeg)
                {
                    parsingAttributes = true;
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
                        _locationEquipment[location] = new List<string>();
                }
                continue;
            }

            // Add equipment to current location
            if (currentLocation.HasValue && !line.Contains("-Empty-"))
            {
                if (parsingAttributes)
                {
                    var keyValue = line.Split(':', 2);
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();
                        _additionalAttributes[key] = value;
                    }
                }
                else
                {
                    _locationEquipment[currentLocation.Value].Add(line.Trim());
                }
            }
        }
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